using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace XRayBuilder.Core.DataSources.Amazon
{
    public interface IAmazonInfoParser
    {
        Task<AmazonInfoParser.InfoResponse> GetAndParseAmazonDocument(string amazonUrl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a book's description, image URL, and rating from the Amazon document
        /// </summary>
        AmazonInfoParser.InfoResponse ParseAmazonDocument(HtmlDocument bookDoc);
    }
}