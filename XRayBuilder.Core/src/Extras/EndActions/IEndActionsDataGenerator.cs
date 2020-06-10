using System;
using System.Threading;
using System.Threading.Tasks;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Model;
using XRayBuilder.Core.Unpack;

namespace XRayBuilder.Core.Extras.EndActions
{
    public interface IEndActionsDataGenerator
    {
        /// <summary>
        /// Generate the necessities for the old format
        /// TODO Remove anything that gets generated for the new version
        /// </summary>
        Task<EndActionsDataGenerator.Response> GenerateOld(BookInfo curBook, EndActionsDataGenerator.Settings settings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate necessities for the new format (which includes running <see cref="EndActionsDataGenerator.GenerateOld"/> automatically)
        /// </summary>
        Task<EndActionsDataGenerator.Response> GenerateNewFormatData(
            BookInfo curBook,
            EndActionsDataGenerator.Settings settings,
            ISecondarySource dataSource,
            AuthorProfileGenerator.Response authorProfile,
            Func<string, string, string> asinPrompt,
            IMetadata metadata,
            IProgressBar progress,
            CancellationToken cancellationToken = default);
    }
}