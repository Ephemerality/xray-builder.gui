// Based on KFX handling from jhowell's KFX in/output plugins (https://www.mobileread.com/forums/showthread.php?t=272407)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.IonDotnet;
using Amazon.IonDotnet.Builders;
using Amazon.IonDotnet.Tree;
using Amazon.IonDotnet.Tree.Impl;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;
using XRayBuilder.Core.Libraries.IO.Extensions;

namespace XRayBuilder.Core.Unpack.KFX
{
    public class YjContainer : IMetadata
    {
        public EntityCollection Entities { get; set; } = new EntityCollection();

        public enum ContainerFormat
        {
            KfxUnknown = 1,
            Kpf,
            KfxMain,
            KfxMetadata,
            KfxAttachable
        }

        public long RawMlSize { get; private set; }
        public Image CoverImage { get; private set; }

        public bool IsAzw3
        {
            get => false;
            set => throw new NotSupportedException();
        }
        public string Asin => Metadata.Asin;
        public string Author => Metadata.Author;
        public string CdeContentType => Metadata.CdeContentType;
        public string DbName => Metadata.AssetId;
        public string Title => Metadata.Title;
        public string UniqueId => null;
        public bool CanModify => false;

        private KfxMetadata Metadata { get; set; }
        private class KfxMetadata
        {
            public string Asin { get; set; }
            public string AssetId { get; set; }
            public string Author { get; set; }
            public string CdeContentType { get; set; }
            public string ContentId { get; set; }
            public string CoverImage { get; set; }
            public string IssueDate { get; set; }
            public string Language { get; set; }
            public string Publisher { get; set; }
            public string Title { get; set; }
        }

        protected void SetMetadata()
        {
            // This is definitely going to break
            // TODO: Handle other ids too, also consider multiple authors
            var metadata = Entities.Where(entity => entity.FragmentType == KfxSymbols.BookMetadata)
                .Select(entity => entity.Value).OfType<IonStruct>()
                .Select(s => s.First()).OfType<IonList>()
                .SelectMany(list => list).OfType<IonStruct>()
                .Where(s => ((IonString) s.First()).StringValue == "kindle_title_metadata")
                .Select(md => (IonList) md.GetField(KfxSymbols.Metadata))
                .Single()
                .Cast<IonStruct>()
                .Where(s => s.GetField(KfxSymbols.Value) is IonString)
                .ToDictionary(s => s.GetField(KfxSymbols.Key).StringValue, s => s.GetField(KfxSymbols.Value).StringValue);

            Metadata = new KfxMetadata
            {
                Asin = metadata.GetOrDefault("ASIN"),
                AssetId = metadata.GetOrDefault("asset_id"),
                Author = metadata.GetOrDefault("author"),
                CdeContentType = metadata.GetOrDefault("cde_content_type"),
                ContentId = metadata.GetOrDefault("content_id"),
                CoverImage = metadata.GetOrDefault("cover_image"),
                IssueDate = metadata.GetOrDefault("issue_date"),
                Language = metadata.GetOrDefault("language"),
                Publisher = metadata.GetOrDefault("publisher"),
                Title = metadata.GetOrDefault("title")
            };
        }

        /// <summary>
        /// Not needed as we will crash during load if DRM is detected
        /// </summary>
        public void CheckDrm() { }

        public byte[] GetRawMl()
        {
            throw new NotSupportedException();
        }

        public Stream GetRawMlStream() => new MemoryStream(new byte[0]);

        public void SaveRawMl(string path)
        {
            throw new NotSupportedException();
        }

        public void UpdateCdeContentType() => throw new NotSupportedException();
        public void Save(Stream stream) => throw new NotSupportedException();
        public void SetAsin(string asin) => throw new NotSupportedException();

        public bool RawMlSupported { get; } = false;

        public void GetBookNavigation()
        {
            var bookNav = Entities.ValueOrDefault<IonList>("$389");
            var readingOrderId = GetReadingOrderIds().Single();
            if (bookNav != null)
            {
                foreach (var nav in bookNav.OfType<IonStruct>())
                {
                    if (nav.GetField(KfxSymbols.ReadingOrderName).SymbolValue.Sid != readingOrderId)
                        continue;
                    var navContainers = (IonList) nav.GetField(KfxSymbols.NavContainers);
                    if (navContainers == null)
                        continue;

                    //if isinstance(nav_container, IonSymbol):
                    //  nav_container = self.fragments[YJFragmentKey(ftype = "$391", fid = nav_container)].value
                    //  inline_nav_containers = False

                    foreach (var navContainer in navContainers.OfType<IonStruct>())
                    {
                        if (navContainer.GetField(KfxSymbols.NavType).SymbolValue.Sid != KfxSymbolTable.KfxSymbolIds[KfxSymbols.Toc])
                            continue;
                        //var containerName = navContainer.GetById<IonSymbol>(239);
                        var chapterList = (IonList) navContainer.GetField(KfxSymbols.Entries);

                    }
                }
            }

            throw new Exception($"Unable to locate book navigation for reading order {readingOrderId}");
        }

        public IEnumerable<int> GetReadingOrderIds()
        {
            var orders = GetReadingOrders();
            return orders.OfType<IonStruct>()
                .Select(order => order.GetField(KfxSymbols.ReadingOrderName)?.SymbolValue.Sid)
                .Where(id => id != null)
                .Cast<int>();
        }

        public IonList GetReadingOrders()
        {
            var docData = Entities.SingleOrDefault("$538");
            if (docData?.Value is IonStruct docStruct)
                return (IonList) docStruct.GetField(KfxSymbols.ReadingOrders);

            throw new NotImplementedException();

            //metadata = self.fragments.get("$258", first = True)
            //return [] if metadata is None else metadata.value.get("$169", [])
        }

        public void Dispose()
        {
            CoverImage?.Dispose();
        }
    }

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
        private readonly int[] _allowedVersions = { 1 };

        public string[] RawFragmentTypes = { KfxSymbols.Bcrawmedia, KfxSymbols.Bcrawfont };

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
            var containerInfoData = new MemoryStream(stream.ReadBytes((int)Length - 10));
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
