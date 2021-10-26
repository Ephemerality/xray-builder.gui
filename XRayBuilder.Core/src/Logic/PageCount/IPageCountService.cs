using Ephemerality.Unpack;

namespace XRayBuilder.Core.Logic.PageCount
{
    public interface IPageCountService
    {
        /// <summary>
        /// Estimates the number of pages in the book described by <paramref name="metadata"/>
        /// </summary>
        int EstimatePageCount(IMetadata metadata);
    }
}