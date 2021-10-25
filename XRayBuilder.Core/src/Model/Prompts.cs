namespace XRayBuilder.Core.Model
{
    public delegate PromptResultYesNo YesNoPrompt(string title, string message, PromptType type);

    public delegate PromptResultYesNoCancel YesNoCancelPrompt(string title, string message, PromptType type);

    public enum PromptResultYesNo
    {
        No,
        Yes
    }

    public enum PromptResultYesNoCancel
    {
        No,
        Yes,
        Cancel
    }

    public enum PromptType
    {
        Info,
        Warning,
        Error
    }
}