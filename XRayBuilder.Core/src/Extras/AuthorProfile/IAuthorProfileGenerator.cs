using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XRayBuilder.Core.Libraries.Progress;

namespace XRayBuilder.Core.Extras.AuthorProfile
{
    public interface IAuthorProfileGenerator
    {
        Task<AuthorProfileGenerator.Response> GenerateAsync(AuthorProfileGenerator.Request request, [CanBeNull] Func<string, bool> editBioCallback, IProgressBar progress = null, CancellationToken cancellationToken = default);
    }
}