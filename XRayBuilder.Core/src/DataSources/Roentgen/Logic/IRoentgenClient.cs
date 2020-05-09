using System.Threading;
using System.Threading.Tasks;
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
        Task<Term[]> DownloadTermsAsync(string asin, CancellationToken cancellationToken);
        Task<EndActions> DownloadEndActionsAsync(string asin, string regionTld, CancellationToken cancellationToken);
        Task<AuthorProfile> DownloadAuthorProfileAsync(string asin, string regionTld, CancellationToken cancellationToken);
    }
}