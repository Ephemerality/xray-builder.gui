// Based on KFX handling from jhowell's KFX in/output plugins (https://www.mobileread.com/forums/showthread.php?t=272407)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon.IonDotnet.Tree.Impl;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Enumerables.Extensions;

namespace XRayBuilder.Core.Unpack.KFX
{
    public class YjContainer : IMetadata
    {
        public EntityCollection Entities { get; } = new EntityCollection();

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
            // TODO: Handle other ids too, also consider multiple authors
            var metadata = Entities
                .ValueOrDefault<IonStruct>(KfxSymbols.BookMetadata)
                ?.OfType<IonList>()
                .FirstOrDefault()
                ?.OfType<IonStruct>()
                .Where(metadataSet => ((IonString) metadataSet.First()).StringValue == "kindle_title_metadata")
                .Select(md => (IonList) md.GetField(KfxSymbols.Metadata))
                .FirstOrDefault()
                ?.OfType<IonStruct>()
                .Where(s => s.GetField(KfxSymbols.Value) is IonString)
                .ToDictionary(metadataValue => metadataValue.GetField(KfxSymbols.Key).StringValue,
                    metadataValue => metadataValue.GetField(KfxSymbols.Value).StringValue);

            if (metadata == null)
                throw new Exception("Metadata not found");

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
        public void CheckDrm()
        {
        }

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

        [CanBeNull]
        public IonList GetDefaultToc()
        {
            var bookNav = Entities.ValueOrDefault<IonList>(KfxSymbols.BookNavigation);
            if (bookNav == null)
                return null;

            // Find default TOC from nav containers
            var chapterList = bookNav.OfType<IonStruct>()
                .Where(nav => nav.GetField(KfxSymbols.ReadingOrderName).StringValue == KfxSymbols.Default)
                .Select(nav => (IonList) nav.GetField(KfxSymbols.NavContainers))
                .Where(navContainers => navContainers != null)
                .SelectMany(navContainers => navContainers.OfType<IonStruct>())
                .Where(navContainer => navContainer.GetField(KfxSymbols.NavType).StringValue == KfxSymbols.Toc)
                .Select(toc => (IonList) toc.GetField(KfxSymbols.Entries))
                .FirstOrDefault();

            return chapterList;
        }

        public IEnumerable<string> GetOrderedSectionNames()
        {
            var defaultReadingOrder = GetReadingOrders().FirstOrDefault();

            return defaultReadingOrder != null
                ? ((IonList) defaultReadingOrder.GetField(KfxSymbols.Sections))
                    .Select(section => section.StringValue)
                : Enumerable.Empty<string>();
        }

        public IonList GetReadingOrders()
        {
            var docData = Entities.ValueOrDefault<IonStruct>(KfxSymbols.DocumentData);
            if (docData != null)
                return (IonList) docData.GetField(KfxSymbols.ReadingOrders);
                
            //todo find a case where the readingorders are stored in aa metadata fragment
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            CoverImage?.Dispose();
        }
    }
}