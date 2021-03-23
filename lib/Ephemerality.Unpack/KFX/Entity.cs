using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree;
using Amazon.IonDotnet.Tree.Impl;
using Ephemerality.Unpack.Extensions;

namespace Ephemerality.Unpack.KFX
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Entity
    {
        public string FragmentId { get; set; }
        public string FragmentType { get; set; }
        public string Signature { get; }
        public ushort Version { get; }
        public uint Length { get; }
        public IIonValue Value { get; }

        private const string EntitySignature = "ENTY";
        private const int MinHeaderLength = 10;
        private readonly int[] _allowedVersions = {1};

        private readonly string[] RawFragmentTypes = {KfxSymbols.Bcrawmedia, KfxSymbols.Bcrawfont};

        private string DebuggerDisplay => $"{FragmentType} - {FragmentId}";

        public Entity(Stream stream, int id, int type, ISymbolTable symbolTable, IonLoader loader)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            Signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (Signature != EntitySignature)
                throw new Exception("Invalid signature");

            Version = reader.ReadUInt16();
            if (!_allowedVersions.Contains(Version))
                throw new Exception($"Version not supported ({Version})");

            Length = reader.ReadUInt32();
            if (Length < MinHeaderLength)
                throw new Exception("Header too short");

            // Duplicated in KfxContainer
            // 10 = number of bytes read so far
            var containerInfoData = new MemoryStream(stream.ReadBytes((int) Length - 10));
            var entityInfo = loader.LoadSingle<IonStruct>(containerInfoData);
            if (entityInfo == null)
                throw new Exception("Bad container or something");

            var compressionType = entityInfo.GetField(KfxSymbols.BcComprType).IntValue;
            if (compressionType != KfxContainer.DefaultCompressionType)
                throw new Exception($"Unexpected bcComprType ({compressionType})");

            var drmScheme = entityInfo.GetField(KfxSymbols.BcDrmScheme).IntValue;
            if (drmScheme != KfxContainer.DefaultDrmScheme)
                throw new Exception($"Unexpected bcDRMScheme ({drmScheme})");

            FragmentId = symbolTable.FindKnownSymbol(id);
            FragmentType = symbolTable.FindKnownSymbol(type);

            Value = RawFragmentTypes.Contains(FragmentType)
                ? new IonBlob(new ReadOnlySpan<byte>(stream.ReadToEnd()))
                : ((IonDatagram) loader.Load(stream.ReadToEnd())).Single();

            // Skipping annotation handling for now

            //if ftype == fid and ftype in ROOT_FRAGMENT_TYPES and not self.pure:

            //fid = "$348"

            //return YJFragment(fid = fid if fid != "$348" else None, ftype = ftype, value = self.value)
        }
    }
}