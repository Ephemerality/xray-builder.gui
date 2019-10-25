using System.Collections.Generic;
using XRayBuilderGUI.Libraries;

namespace XRayBuilderGUI.UI.Preview.Logic
{
    public class PreviewProviderFactory : Factory<PreviewProviderFactory.PreviewType, PreviewProvider>
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

        protected override Dictionary<PreviewType, PreviewProvider> Dictionary { get; }

        public enum PreviewType
        {
            AuthorProfile,
            EndActions,
            StartActions,
            XRay
        }
    }
}