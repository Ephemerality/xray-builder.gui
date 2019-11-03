using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilderGUI.Extras.AuthorProfile
{
    public interface IAuthorProfileGenerator
    {
        Task<AuthorProfileGenerator.Response> GenerateAsync(AuthorProfileGenerator.Request request, CancellationToken cancellationToken = default);
    }
}