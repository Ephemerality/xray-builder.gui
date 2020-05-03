namespace XRayBuilder.Core.Extras.EndActions
{
    public interface IEndActionsArtifactService
    {
        string GenerateNew(EndActionsArtifactService.Request request);
        string GenerateOld(EndActionsArtifactService.Request request);
    }
}