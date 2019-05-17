using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilderGUI
{
    public interface IAuthorProfileGenerator
    {
        Task<AuthorProfileGenerator.Response> GenerateAsync(AuthorProfileGenerator.Request request, CancellationToken cancellationToken = default);
    }
}