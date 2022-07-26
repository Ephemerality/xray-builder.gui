using System.Threading;
using System.Threading.Tasks;
using Ephemerality.Unpack;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Prompt;

namespace XRayBuilder.Core.Logic
{
    public interface IMetadataService
    {
        [ItemCanBeNull]
        Task<IMetadata> GetAndValidateMetadataAsync(string mobiFile, YesNoCancelPrompt yesNoCancelPrompt, CancellationToken cancellationToken);
        Task CheckAndFixIncorrectAsinOrThrowAsync(IMetadata metadata, string bookPath, YesNoCancelPrompt yesNoCancelPrompt, CancellationToken cancellationToken);
        void EbokTagPromptOrThrow(IMetadata md, string bookPath, YesNoCancelPrompt yesNoCancelPrompt);
    }
}