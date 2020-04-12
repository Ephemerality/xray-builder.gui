// Based on KFX handling from jhowell's KFX in/output plugins (https://www.mobileread.com/forums/showthread.php?t=272407)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree.Impl;
using Newtonsoft.Json;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.IO.Extensions;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Unpack.KFX
{
    public sealed class KfxContainer : YjContainer
    {
        public const int DefaultCompressionType = 0;
        public const int DefaultDrmScheme = 0;
        private const int DefaultChunkSize = 4096;

        private IonList FormatCapabilities { get; set; } = IonList.NewNull();

        /// <summary>
        /// Storyline, Section, DocumentData
        /// </summary>
        private readonly int[] _kfxMainContainerFragmentIdNums = { 259, 260, 538 };

        /// <summary>
        /// Metadata, ContainerEntityMap, BookMetadata, ContentFeatures
        /// </summary>
        private readonly int[] _kfxMetadataContainerFragmentIdNums = { 258, 419, 490, 585 };

        /// <summary>
        /// BcRawMedia
        /// </summary>
        private readonly int[] _kfxAttachableContainerFragmentIdNums = { 417 };

        public readonly KfxContainerInfo ContainerInfo;

        public KfxContainer(Stream fs)
        {
            var loader = IonLoader.WithReaderOptions(new ReaderOptions
            {
                Encoding = Encoding.UTF8,
                Format = ReaderFormat.Detect,
                Catalog = KfxSymbolTable.GetCatalog()
            });

            var header = new KfxHeader(fs);
            var containerInfoData = new MemoryStream(fs.ReadBytes((int) header.ContainerInfoOffset, (int) header.ContainerInfoLength, SeekOrigin.Begin));

            var containerInfo = loader.LoadSingle<IonStruct>(containerInfoData);
            if (containerInfo == null)
                throw new Exception("Bad container or something");

            var containerId = containerInfo.GetField(KfxSymbols.BcContId).StringValue;

            var compressionType = containerInfo.GetField(KfxSymbols.BcComprType).IntValue;
            if (compressionType != DefaultCompressionType)
                throw new Exception($"Unexpected bcComprType ({compressionType})");

            var drmScheme = containerInfo.GetField(KfxSymbols.BcDrmScheme).IntValue;
            if (drmScheme != DefaultDrmScheme)
                throw new Exception($"Unexpected bcDRMScheme ({drmScheme})");

            var docSymbolOffset = containerInfo.GetField(KfxSymbols.BcDocSymbolOffset);
            var docSymbolLength = containerInfo.GetField(KfxSymbols.BcDocSymbolLength);
            ISymbolTable docSymbols = null;
            if (docSymbolLength.LongValue > 0)
            {
                var docSymbolData = new MemoryStream(fs.ReadBytes(docSymbolOffset.IntValue, docSymbolLength.IntValue, SeekOrigin.Begin));
                loader.Load(docSymbolData, out docSymbols);
            }

            var chunkSize = containerInfo.GetField(KfxSymbols.BcChunkSize).IntValue;
            if (chunkSize != DefaultChunkSize)
                throw new Exception($"Unexpected bcChunkSize in container {containerId} info ({chunkSize})");

            if (header.Version > 1)
            {
                var formatCapabilitiesOffset = containerInfo.GetField(KfxSymbols.BcFCapabilitiesOffset).IntValue;
                var formatCapabilitiesLength = containerInfo.GetField(KfxSymbols.BcFCapabilitiesLength).IntValue;
                if (formatCapabilitiesLength > 0)
                {
                    var formatCapabilitiesData = new MemoryStream(fs.ReadBytes(formatCapabilitiesOffset, formatCapabilitiesLength, SeekOrigin.Begin));
                    FormatCapabilities = ((IonDatagram) loader.Load(formatCapabilitiesData)).Single() as IonList;
                }
            }

            var indexTableOffset = containerInfo.GetField(KfxSymbols.BcIndexTabOffset).IntValue;
            var indexTableLength = containerInfo.GetField(KfxSymbols.BcIndexTabLength).IntValue;

            // Python checks for extra info in container

            fs.Seek(header.Length, SeekOrigin.Begin);
            var payloadSha1 = fs.ReadToEnd().Sha1().ToHexString().ToLowerInvariant();

            fs.Seek(header.ContainerInfoOffset + header.ContainerInfoLength, SeekOrigin.Begin);
            var kfxGenInfoData = Encoding.UTF8.GetString(fs.ReadBytes((int) (header.Length - header.ContainerInfoOffset - header.ContainerInfoLength)));
            var kfxGenInfo = JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(kfxGenInfoData)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (kfxGenInfo.GetOrDefault("kfxgen_payload_sha1") != payloadSha1)
                throw new Exception($"Incorrect kfxgen_payload_sha1 in container {containerId}");
            if (kfxGenInfo.GetOrDefault("kfxgen_acr") != containerId)
                throw new Exception($"Unexpected kfxgen_acr in container {containerId}");

            var typeNums = new HashSet<int>();
            if (indexTableLength > 0)
            {
                var entityTable = fs.ReadBytes(indexTableOffset, indexTableLength, SeekOrigin.Begin);
                using var reader = new BinaryReader(new MemoryStream(entityTable), Encoding.UTF8, true);
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var id = reader.ReadInt32();
                    var type = reader.ReadInt32();
                    var entityOffset = (int) reader.ReadInt64();
                    var entityLength = (int) reader.ReadInt64();

                    typeNums.Add(type);

                    var entityStart = (int) header.Length + entityOffset;
                    if (entityStart + entityLength > fs.Length)
                        throw new Exception($"Container {containerId} is not large enough for entity end (offset {entityStart + entityLength})");

                    var entityData = new MemoryStream(fs.ReadBytes(entityStart, entityLength, SeekOrigin.Begin));
                    Entities.Add(new Entity(entityData, id, type, docSymbols, loader));
                }
            }

            ContainerFormat containerFormat;
            if (typeNums.Intersect(_kfxMainContainerFragmentIdNums).Any())
                containerFormat = ContainerFormat.KfxMain;
            else if (typeNums.Intersect(_kfxMetadataContainerFragmentIdNums).Any())
                containerFormat = ContainerFormat.KfxMetadata;
            else if (typeNums.Intersect(_kfxAttachableContainerFragmentIdNums).Any())
                containerFormat = ContainerFormat.KfxAttachable;
            else
                containerFormat = ContainerFormat.KfxUnknown;

            var kfxAppVersion = kfxGenInfo.GetOrDefault("appVersion") ?? kfxGenInfo.GetOrDefault("kfxgen_application_version");
            var kfxPackageVersion = kfxGenInfo.GetOrDefault("buildVersion") ?? kfxGenInfo.GetOrDefault("kfxgen_package_version");

            ContainerInfo = new KfxContainerInfo
            {
                Header = header,
                ContainerId = containerId,
                ChunkSize = chunkSize,
                CompressionType = compressionType,
                DrmScheme = drmScheme,
                KfxGenApplicationVersion = kfxAppVersion,
                KfxGenPackageVersion = kfxPackageVersion,
                ContainerFormat = containerFormat
            };

            SetMetadata();
        }
    }

    public class KfxContainerInfo
    {
        public KfxHeader Header { get; set; }
        public string ContainerId { get; set; }
        public int ChunkSize { get; set; }
        public int CompressionType { get; set; }
        public int DrmScheme { get; set; }
        public string KfxGenApplicationVersion { get; set; }
        public string KfxGenPackageVersion { get; set; }
        public YjContainer.ContainerFormat ContainerFormat { get; set; }
    }

    public class KfxHeader
    {
        public string Signature { get; }
        public ushort Version { get; }
        public uint Length { get; }
        public uint ContainerInfoOffset { get; }
        public uint ContainerInfoLength { get; }

        private const string DrmSignature = "?DRM";
        private const string KfxSignature = "CONT";
        private const int MinHeaderLength = 18;
        private readonly int[] _allowedVersions = { 1, 2 };

        public KfxHeader(Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            Signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
            switch (Signature)
            {
                case KfxSignature:
                    break;
                case DrmSignature:
                    throw new Exception("DRM-protected books are not supported");
                default:
                    throw new Exception("Book is not in KFX format");
            }

            Version = reader.ReadUInt16();
            if (!_allowedVersions.Contains(Version))
                throw new Exception($"KFX version not supported ({Version})");

            Length = reader.ReadUInt32();
            if (Length < MinHeaderLength)
                throw new Exception("Invalid KFX: header too short");

            ContainerInfoOffset = reader.ReadUInt32();
            ContainerInfoLength = reader.ReadUInt32();
        }
    }
}
