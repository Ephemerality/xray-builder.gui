using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.DataSources.Logic
{
    public interface IBookSearchService
    {
        /// <summary>
        /// Given a set of  <paramref name="metadata"/>, searches <paramref name="dataSource"/> for matching books and returns them in descending order of likelihood
        /// </summary>
        Task<BookInfo[]> SearchSecondarySourceAsync(ISecondarySource dataSource, IMetadata metadata, CancellationToken cancellationToken = default);
    }
}