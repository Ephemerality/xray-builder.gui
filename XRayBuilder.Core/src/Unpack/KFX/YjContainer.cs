// Based on KFX handling from jhowell's KFX in/output plugins (https://www.mobileread.com/forums/showthread.php?t=272407)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon.IonDotnet.Tree;
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

        public Image CoverImage
        {
            get
            {
                // todo move this out of the property
                if (Metadata?.CoverImage == null)
                    return null;

                var resourceInfo = Entities
                    .FirstOrDefault(f => f.FragmentType == KfxSymbols.ExternalResource && f.FragmentId == Metadata.CoverImage)
                    ?.Value;
                if (resourceInfo == null)
                    return null;

                var resourceId = resourceInfo.GetField(KfxSymbols.Location).StringValue;
                var format = resourceInfo.GetField(KfxSymbols.Format).StringValue;

                var imageFragment = Entities
                    .FirstOrDefault(f => f.FragmentType == KfxSymbols.Bcrawmedia && f.FragmentId == resourceId)
                    ?.Value;
                if (imageFragment == null || !(imageFragment is IonBlob blob))
                    return null;

                try
                {
                    using var ms = new MemoryStream(blob.Bytes().ToArray());
                    return new Bitmap(ms);
                }
                catch (Exception e)
                {
                    throw new Exception($"Unsupported image format: {format}", e);
                }
            }
        }

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

            //todo find a case where the readingorders are stored in a metadata fragment
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retrieve ordered content chunks from the container
        /// Translated more or less directly from the KFX Input plugin by jhowell, yj_position_location.py
        /// </summary>
        /// TODO Tidy up, remove double-nested local functions
        public List<ContentChunk> GetContentChunks()
        {
            var posInfo = new List<ContentChunk>();
            var sectionPosInfo = new List<ContentChunk>();
            var eidSection = new Dictionary<int, string>();
            var eidStartPos = new Dictionary<int, int>();
            var sectionsByStory = new Dictionary<string, string>();
            var storiesBySection = new Dictionary<string, string>();
            var processedStoryNames = new List<string>();
            var cpiPid = 0;
            var cpiPidForOffset = 0;

            void CollectSectionPositionInfo(string name)
            {
                var pendingStoryNames = new List<string>();

                void ExtractPositionData(IIonValue value, int? currentEid, string contentKey, int? listIndex, object listMax, bool advance)
                {
                    void HaveContent(int? eid, int length, bool _advance, string contentName = null, string contentText = null, bool allowZero = true, bool matchZeroLen = false)
                    {
                        if (eid == null)
                            return;

                        if (!eidStartPos.ContainsKey(eid.Value))
                            eidStartPos[eid.Value] = cpiPidForOffset;

                        var eidOffset = cpiPidForOffset - eidStartPos[eid.Value];

                        if (_advance)
                            cpiPidForOffset += length;

                        var last = sectionPosInfo.LastOrDefault();
                        if (last?.Eid == eid)
                        {
                            last.Length += length;
                            cpiPid += length;
                            eidOffset += length;
                            length = 0;

                            if (last.Length == 0 || !allowZero)
                                return;
                        }

                        sectionPosInfo.Add(new ContentChunk
                        {
                            Pid = cpiPid,
                            Eid = eid.Value,
                            EidOffset = eidOffset,
                            Length = length,
                            Name = eidSection[eid.Value],
                            MatchZeroLen = matchZeroLen,
                            ContentName = contentName,
                            ContentText = contentText
                        });

                        cpiPid += length;
                    }

                    var parentEid = (int?) null;
                    if (value is IonList ionList)
                    {
                        foreach (var (v, i) in ionList.Select((v, i) => (v, i)))
                        {
                            var valueToExtract = v;
                            if (new[] {KfxSymbols.ContentList, KfxSymbols.Plugin}.Contains(contentKey) && v is IonSymbol listSymbol) // todo && is_kpf_prepub
                                // todo confirm this actually works
                                valueToExtract = Entities.First(frag => frag.FragmentType == KfxSymbols.Structure && frag.FragmentId == listSymbol.SymbolValue.Text).Value;
                            ExtractPositionData(valueToExtract, currentEid, contentKey, i, ionList.Count - 1, advance);
                        }
                    }
                    else if (value is IonStruct ionStruct)
                    {
                        if (contentKey != KfxSymbols.Storyline)
                        {
                            var eid = ionStruct.GetField(KfxSymbols.Id) ?? ionStruct.GetField(KfxSymbols.KfxId);
                            if (eid != null)
                            {
                                parentEid = currentEid;
                                currentEid = eid.IntValue;
                                if (eidSection.ContainsKey(currentEid.Value))
                                {
                                    if (eidSection[currentEid.Value] == name)
                                        throw new Exception($"Duplicate eid {currentEid} in section {name}");
                                    throw new Exception($"Duplicate eid {currentEid} in {name} and {eidSection[currentEid.Value]}");
                                }

                                eidSection[currentEid.Value] = name;
                            }
                        }

                        var annotationType = ionStruct.GetField(KfxSymbols.AnnotationType);
                        var type = ionStruct.GetField(KfxSymbols.Type);
                        if (new[] {KfxSymbols.Text, KfxSymbols.Container, KfxSymbols.Image}.Contains(type?.StringValue)
                            && parentEid != null && listIndex != null
                            && contentKey == KfxSymbols.ContentList
                            && ionStruct.GetField(KfxSymbols.Render)?.StringValue == KfxSymbols.Inline)
                        {
                            HaveContent(parentEid, listIndex == 0 ? -1 : 0, advance);
                        }

                        var saveCpiPidForOffset = cpiPidForOffset;

                        if (new[] {KfxSymbols.Image, KfxSymbols.Kvg, KfxSymbols.Plugin, KfxSymbols.HorizontalRule}.Contains(type?.StringValue))
                            HaveContent(currentEid, 1, advance);
                        else if (new[] {KfxSymbols.Text, KfxSymbols.Container, KfxSymbols.ListItem}.Contains(type?.StringValue))
                        {
                            if (new[] {KfxSymbols.Content, KfxSymbols.ContentList, KfxSymbols.StoryName}.All(ct => !ionStruct.ContainsField(ct)))
                                HaveContent(currentEid, 1, advance); // todo matchZeroLen: isKpr321 && type?.StringValue == KfxSymbols.Text
                        }

                        foreach (var pageTemplate in ionStruct.Where(s => s.FieldNameSymbol.Text == KfxSymbols.PageTemplates).OfType<IonList>())
                            ExtractPositionData(pageTemplate.First(), currentEid, KfxSymbols.PageTemplates, null, null, advance);

                        if (ionStruct.ContainsField(KfxSymbols.ContentList) && !new[] {KfxSymbols.Kvg, KfxSymbols.Plugin}.Contains(type?.StringValue))
                            HaveContent(currentEid, 1, advance && !new[] {KfxSymbols.Math, KfxSymbols.Mathsegment}.Contains(ionStruct.GetField(KfxSymbols.YjClassification)?.StringValue));

                        if (ionStruct.ContainsField(KfxSymbols.Content) && !new[] {KfxSymbols.AltText, KfxSymbols.Mathml}.Contains(annotationType?.StringValue))
                        {
                            var contentStruct = ionStruct.GetField(KfxSymbols.Content);
                            var content = Entities
                                .First(fragment => fragment.FragmentType == KfxSymbols.Content && fragment.FragmentId == contentStruct.GetField("name").StringValue)
                                .Value
                                .GetField(KfxSymbols.ContentList)
                                .GetElementAt(contentStruct.GetField(KfxSymbols.Index).IntValue);
                            HaveContent(currentEid, content.StringValue.Length, advance, contentStruct.GetField("name").StringValue, content.StringValue);
                        }

                        if (ionStruct.ContainsField(KfxSymbols.Annotations))
                            ExtractPositionData(ionStruct.GetField(KfxSymbols.Annotations), currentEid, KfxSymbols.Annotations, null, null, advance);

                        if (ionStruct.ContainsField(KfxSymbols.AltContent))
                            ExtractPositionData(Entities.First(fragment => fragment.FragmentType == KfxSymbols.Storyline && fragment.FragmentId == ionStruct.GetField(KfxSymbols.AltContent).StringValue).Value, null, KfxSymbols.Storyline, null, null, advance);

                        if (ionStruct.ContainsField(KfxSymbols.ContentList))
                        {
                            var contentListValue = ionStruct.GetField(KfxSymbols.ContentList);
                            if (type?.StringValue == KfxSymbols.Plugin)
                                ExtractPositionData(contentListValue, currentEid, type.StringValue, null, null, advance);
                            else if (type?.StringValue == KfxSymbols.Kvg)
                                ExtractPositionData(contentListValue, null, KfxSymbols.ContentList, null, null, false);
                            else
                                ExtractPositionData(contentListValue, currentEid, KfxSymbols.ContentList, null, null, advance);
                        }

                        if (ionStruct.ContainsField(KfxSymbols.Content) && !new[] {KfxSymbols.AltText, KfxSymbols.Mathml}.Contains(annotationType?.StringValue))
                        {
                            var contentValue = ionStruct.GetField(KfxSymbols.Content);
                            if (!(contentValue is IonStruct))
                                ExtractPositionData(contentValue, currentEid, KfxSymbols.Content, null, null, advance);
                        }

                        if (ionStruct.ContainsField(KfxSymbols.StoryName) && contentKey != KfxSymbols.Storyline)
                        {
                            HaveContent(currentEid, 1, advance);

                            var storyNameValue = ionStruct.GetField(KfxSymbols.StoryName).StringValue;
                            storiesBySection[name] = storyNameValue;
                            sectionsByStory[storyNameValue] = name;

                            // if self.is_conditional_structure:
                            //     if fv not in pending_story_names:
                            //         pending_story_names.append(fv)
                            // else:
                            if (!processedStoryNames.Contains(storyNameValue))
                            {
                                var fragment = Entities.First(frag => frag.FragmentType == KfxSymbols.Storyline && frag.FragmentId == storyNameValue);
                                ExtractPositionData(fragment.Value, null, KfxSymbols.Storyline, null, null, advance);
                                processedStoryNames.Add(storyNameValue);
                            }
                        }

                        var extraItemsIgnoredTypes = new[]
                        {
                            KfxSymbols.AltContent, KfxSymbols.AltText, KfxSymbols.Annotations, KfxSymbols.Content,
                            KfxSymbols.ContentList, KfxSymbols.PageTemplates, KfxSymbols.ReadingOrderSwitchMap, KfxSymbols.ShapeList, KfxSymbols.StoryName,
                            "yj.dictionary.term", "yj.dictionary.unnormalized_term"
                        };
                        var extraItems = ionStruct.Where(s => !(s is IonString) && !extraItemsIgnoredTypes.Contains(s.FieldNameSymbol.Text));
                        foreach (var extraItem in extraItems)
                            ExtractPositionData(extraItem, currentEid, extraItem.FieldNameSymbol.Text, null, null, advance);

                        if (type?.StringValue != KfxSymbols.Image && ionStruct.GetField(KfxSymbols.Render)?.StringValue == KfxSymbols.Inline && cpiPidForOffset > saveCpiPidForOffset + 1)
                            cpiPidForOffset++;
                    }
                    else if (value is IonSexp)
                    {
                        throw new NotSupportedException();
                    }
                    else if (value is IonString)
                    {
                        throw new NotSupportedException();
                    }
                }

                var section = Entities.FirstOrDefault(fragment => fragment.FragmentType == KfxSymbols.Section && fragment.FragmentId == name);
                if (section == null)
                    return;
                ExtractPositionData(section.Value, null, KfxSymbols.Section, null, null, true);

                // TODO
                if (pendingStoryNames.Any())
                    throw new NotSupportedException();

                posInfo.AddRange(sectionPosInfo);
                sectionPosInfo.Clear();
            }

            var sectionNames = GetOrderedSectionNames();
            foreach (var sectionName in sectionNames)
                CollectSectionPositionInfo(sectionName);

            return posInfo;
        }

        public void Dispose()
        {
            CoverImage?.Dispose();
        }
    }
}