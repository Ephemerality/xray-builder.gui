// Based on KFX handling from jhowell's KFX in/output plugins (https://www.mobileread.com/forums/showthread.php?t=272407)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree.Impl;
using Ephemerality.Unpack.Exceptions;
using Ephemerality.Unpack.Extensions;
using Newtonsoft.Json;

namespace Ephemerality.Unpack.KFX
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
                throw new UnpackException("Bad container or something");

            // Get document symbol offsets by their ID, load the symbols, then reload the container
            var docSymbolOffset = containerInfo.GetById(KfxSymbolTable.KfxSymbolIds[KfxSymbols.BcDocSymbolOffset]);
            var docSymbolLength = containerInfo.GetById(KfxSymbolTable.KfxSymbolIds[KfxSymbols.BcDocSymbolLength]);
            ISymbolTable docSymbols = null;
            if (docSymbolLength.LongValue > 0)
            {
                var docSymbolData = new MemoryStream(fs.ReadBytes(docSymbolOffset.IntValue, docSymbolLength.IntValue, SeekOrigin.Begin));
                loader.Load(docSymbolData, out docSymbols);

                loader = IonLoader.WithReaderOptions(new ReaderOptions
                {
                    Encoding = Encoding.UTF8,
                    Format = ReaderFormat.Detect,
                    InitialSymbolTable = docSymbols
                });
                containerInfoData.Seek(0, SeekOrigin.Begin);
                containerInfo = loader.LoadSingle<IonStruct>(containerInfoData);
            }

            var containerId = containerInfo.GetField(KfxSymbols.BcContId).StringValue;

            var compressionType = containerInfo.GetField(KfxSymbols.BcComprType).IntValue;
            if (compressionType != DefaultCompressionType)
                throw new UnpackException($"Unexpected bcComprType ({compressionType})");

            var drmScheme = containerInfo.GetField(KfxSymbols.BcDrmScheme).IntValue;
            if (drmScheme != DefaultDrmScheme)
                throw new UnpackException($"Unexpected bcDRMScheme ({drmScheme})");

            var chunkSize = containerInfo.GetField(KfxSymbols.BcChunkSize).IntValue;
            if (chunkSize != DefaultChunkSize)
                throw new UnpackException($"Unexpected bcChunkSize in container {containerId} info ({chunkSize})");

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
                throw new UnpackException($"Incorrect kfxgen_payload_sha1 in container {containerId}");
            if (kfxGenInfo.GetOrDefault("kfxgen_acr") != containerId)
                throw new UnpackException($"Unexpected kfxgen_acr in container {containerId}");

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
                        throw new UnpackException($"Container {containerId} is not large enough for entity end (offset {entityStart + entityLength})");

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
}
