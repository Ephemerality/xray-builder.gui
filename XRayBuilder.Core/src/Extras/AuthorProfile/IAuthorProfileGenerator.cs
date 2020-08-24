using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XRayBuilder.Core.Extras.AuthorProfile
{
    public interface IAuthorProfileGenerator
    {
        Task<AuthorProfileGenerator.Response> GenerateAsync(AuthorProfileGenerator.Request request, [CanBeNull] Func<string, bool> editBioCallback, CancellationToken cancellationToken = default);
    }
}