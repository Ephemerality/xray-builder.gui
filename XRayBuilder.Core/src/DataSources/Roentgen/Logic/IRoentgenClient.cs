using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.DataSources.Amazon.Model;
using XRayBuilder.Core.Extras.Artifacts;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.DataSources.Roentgen.Logic
{
    public interface IRoentgenClient
    {
        Task<StartActions> DownloadStartActionsAsync(string asin, string regionTld, CancellationToken cancellationToken);
        Task<NextBookResult> DownloadNextInSeriesAsync(string asin, CancellationToken cancellationToken);
        Task PreloadAsync(string asin, CancellationToken cancellationToken);
        [ItemCanBeNull]
        Task<Term[]> DownloadTermsAsync(string asin, string regionTld, CancellationToken cancellationToken);
        [ItemCanBeNull]
        Task<EndActions> DownloadEndActionsAsync(string asin, string regionTld, CancellationToken cancellationToken);
        [ItemCanBeNull]
        Task<AuthorProfile> DownloadAuthorProfileAsync(string asin, string regionTld, CancellationToken cancellationToken);
    }
}