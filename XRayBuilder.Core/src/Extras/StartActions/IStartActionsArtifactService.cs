using XRayBuilder.Core.Extras.AuthorProfile;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Extras.StartActions
{
    public interface IStartActionsArtifactService
    {
        Artifacts.StartActions GenerateStartActions(BookInfo curBook, AuthorProfileGenerator.Response authorProfile);
    }
}