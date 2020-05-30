using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XRayBuilder.Core.Extras.AuthorProfile
{
    public interface IEndActionsAuthorConverter
    {
        Task<AuthorProfileGenerator.Response> ConvertAsync([NotNull] Artifacts.EndActions endActions, CancellationToken cancellationToken);
    }
}