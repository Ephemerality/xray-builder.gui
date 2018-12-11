using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IonDotnet;
using IonDotnet.Systems;
using IonDotnet.Tree;
using Newtonsoft.Json;

namespace XRayBuilderGUI.Unpack.KFX
{
    public sealed class KfxContainer : YjContainer
    {
        public const int DefaultCompressionType = 0;
        public const int DefaultDrmScheme = 0;
        public const int DefaultChunkSize = 4096;

        public IonList FormatCapabilities { get; set; } = IonList.NewNull();

        public int[] KfxMainContainerFragmentIdNums = { 259, 260, 538 };
        public int[] KfxMetadataContainerFragmentIdNums = { 258, 419, 490, 585 };
        public int[] KfxAttachableContainerFragmentIdNums = { 417 };

        public KfxContainerInfo ContainerInfo;

        public KfxContainer(Stream fs)
        {
            var catalog = new SimpleCatalog();
            catalog.PutTable(YjSymbolTable);

            var loader = IonLoader.WithReaderOptions(new ReaderOptions
            {
                Encoding = Encoding.UTF8,
                Format = ReaderFormat.Detect,
                Catalog = catalog
            });

            var header = new KfxHeader(fs);
            var containerInfoData = new SubStream(fs, header.ContainerInfoOffset, header.ContainerInfoLength);

            var containerInfo = loader.LoadSingle<IonStruct>(containerInfoData);
            if (containerInfo == null)
                throw new Exception("Bad container or something");

            var containerId = containerInfo.GetById<IonString>(409).StringValue;

            var compressionType = containerInfo.GetById<IonInt>(410).IntValue;
            if (compressionType != DefaultCompressionType)
                throw new Exception($"Unexpected bcComprType ({compressionType})");

            var drmScheme = containerInfo.GetById<IonInt>(411).IntValue;
            if (drmScheme != DefaultDrmScheme)
                throw new Exception($"Unexpected bcDRMScheme ({drmScheme})");

            var docSymbolOffset = containerInfo.GetById<IonInt>(415);
            var docSymbolLength = containerInfo.GetById<IonInt>(416);
            ISymbolTable docSymbols = null;
            if (docSymbolLength.LongValue > 0)
            {
                var docSymbolData = new SubStream(fs, docSymbolOffset.LongValue, docSymbolLength.LongValue);
                loader.Load(docSymbolData, out docSymbols);
            }

            var chunkSize = containerInfo.GetById<IonInt>(412).IntValue;
            if (chunkSize != DefaultChunkSize)
                throw new Exception($"Unexpected bcChunkSize in container {containerId} info ({chunkSize})");

            if (header.Version > 1)
            {
                var formatCapabilitiesOffset = containerInfo.GetById<IonInt>(594).LongValue;
                var formatCapabilitiesLength = containerInfo.GetById<IonInt>(595).LongValue;
                if (formatCapabilitiesLength > 0)
                {
                    var formatCapabilitiesData = new SubStream(fs, formatCapabilitiesOffset, formatCapabilitiesLength);
                    FormatCapabilities = loader.Load(formatCapabilitiesData).Single() as IonList;
                }
            }

            var indexTableOffset = containerInfo.GetById<IonInt>(413).IntValue;
            var indexTableLength = containerInfo.GetById<IonInt>(414).IntValue;

            // Python checks for extra info in container

            fs.Seek(header.Length, SeekOrigin.Begin);
            var payloadSha1 = Functions.ByteToHexString(Functions.Sha1(fs.ReadToEnd())).ToLowerInvariant();

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
                using (var reader = new BinaryReader(new MemoryStream(entityTable), Encoding.UTF8, true))
                {
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
            }

            ContainerFormat containerFormat;
            if (typeNums.Intersect(KfxMainContainerFragmentIdNums).Any())
                containerFormat = ContainerFormat.KfxMain;
            else if (typeNums.Intersect(KfxMetadataContainerFragmentIdNums).Any())
                containerFormat = ContainerFormat.KfxMetadata;
            else if (typeNums.Intersect(KfxAttachableContainerFragmentIdNums).Any())
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
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
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
}
