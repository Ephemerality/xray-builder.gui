using System.Collections.Generic;
using XRayBuilder.Core.Libraries;

namespace XRayBuilderGUI.UI.Preview.Model
{
    public sealed class PreviewProviderFactory : Factory<PreviewProviderFactory.PreviewType, PreviewProvider>
    {
        public PreviewProviderFactory(
            // ReSharper disable SuggestBaseTypeForParameter
            PreviewProvider.PreviewProviderAuthorProfile previewProviderAp,
            PreviewProvider.PreviewProviderEndActions previewProviderEa,
            PreviewProvider.PreviewProviderStartActions previewProviderSa,
            PreviewProvider.PreviewProviderXRay previewProviderXr)
        {
            Dictionary = new Dictionary<PreviewType, PreviewProvider>
            {
                {PreviewType.AuthorProfile, previewProviderAp},
                {PreviewType.EndActions, previewProviderEa},
                {PreviewType.StartActions, previewProviderSa},
                {PreviewType.XRay, previewProviderXr}
            };
        }

        protected override IReadOnlyDictionary<PreviewType, PreviewProvider> Dictionary { get; }

        public enum PreviewType
        {
            AuthorProfile,
            EndActions,
            StartActions,
            XRay
        }
    }
}