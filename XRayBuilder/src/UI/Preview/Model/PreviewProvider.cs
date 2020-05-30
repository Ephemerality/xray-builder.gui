using System;
using JetBrains.Annotations;
using SimpleInjector;

namespace XRayBuilderGUI.UI.Preview.Model
{
    public abstract class PreviewProvider
    {
        public abstract PreviewProviderFactory.PreviewType Type { get; }
        public abstract string Name { get; }
        public abstract string FilenameValidator { get; }
        protected abstract Type PreviewFormType { get; }

        private readonly Container _diContainer;

        private PreviewProvider(Container diContainer)
        {
            _diContainer = diContainer;
        }

        public IPreviewForm GenForm() => (IPreviewForm) _diContainer.GetInstance(PreviewFormType);

        [UsedImplicitly]
        public sealed class PreviewProviderAuthorProfile : PreviewProvider
        {
            public PreviewProviderAuthorProfile(Container diContainer) : base(diContainer) { }

            public override PreviewProviderFactory.PreviewType Type { get; } = PreviewProviderFactory.PreviewType.AuthorProfile;
            public override string Name { get; } = "AuthorProfile";
            public override string FilenameValidator { get; } = "AuthorProfile";
            protected override Type PreviewFormType { get; } = typeof(frmPreviewAP);
        }

        [UsedImplicitly]
        public sealed class PreviewProviderEndActions : PreviewProvider
        {
            public PreviewProviderEndActions(Container diContainer) : base(diContainer) { }

            public override PreviewProviderFactory.PreviewType Type { get; } = PreviewProviderFactory.PreviewType.EndActions;
            public override string Name { get; } = "EndActions";
            public override string FilenameValidator { get; } = "EndActions";
            protected override Type PreviewFormType { get; } = typeof(frmPreviewEA);
        }

        [UsedImplicitly]
        public sealed class PreviewProviderStartActions : PreviewProvider
        {
            public PreviewProviderStartActions(Container diContainer) : base(diContainer) { }

            public override PreviewProviderFactory.PreviewType Type { get; } = PreviewProviderFactory.PreviewType.StartActions;
            public override string Name { get; } = "StartActions";
            public override string FilenameValidator { get; } = "StartActions";
            protected override Type PreviewFormType { get; } = typeof(frmPreviewSA);
        }

        [UsedImplicitly]
        public sealed class PreviewProviderXRay : PreviewProvider
        {
            public PreviewProviderXRay(Container diContainer) : base(diContainer) { }

            public override PreviewProviderFactory.PreviewType Type { get; } = PreviewProviderFactory.PreviewType.XRay;
            public override string Name { get; } = "X-Ray";
            public override string FilenameValidator { get; } = "XRAY.entities";
            protected override Type PreviewFormType { get; } = typeof(frmPreviewXR);
        }
    }
}