using System;
using System.IO;
using System.Linq;
using System.Text;
using Ephemerality.Unpack.Exceptions;

namespace Ephemerality.Unpack.KFX
{
    public sealed class KfxHeader
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
                    throw new UnpackException("DRM-protected books are not supported");
                default:
                    throw new UnpackException("Book is not in KFX format");
            }

            Version = reader.ReadUInt16();
            if (!_allowedVersions.Contains(Version))
                throw new UnpackException($"KFX version not supported ({Version})");

            Length = reader.ReadUInt32();
            if (Length < MinHeaderLength)
                throw new UnpackException("Invalid KFX: header too short");

            ContainerInfoOffset = reader.ReadUInt32();
            ContainerInfoLength = reader.ReadUInt32();
        }
    }
}