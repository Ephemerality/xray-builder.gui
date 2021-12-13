using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Prompt;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.XRay.Logic.Build
{
    public interface IXRayBuildService
    {
        /// <summary>
        /// Builds an X-Ray file from the parameters given and returns the path at which the file has been saved (or null if something failed)
        /// </summary>
        Task<XRay> BuildAsync([NotNull] XRayBuildService.Request request, [CanBeNull] YesNoPrompt yesNoPrompt, [CanBeNull] YesNoCancelPrompt yesNoCancelPrompt, [CanBeNull] EditCallback editCallback, [CanBeNull] IProgressBar progress, CancellationToken cancellationToken);
    }
}