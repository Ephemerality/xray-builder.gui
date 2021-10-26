using System.ComponentModel;

namespace XRayBuilder.Core.Model
{
    public delegate PromptResultYesNo YesNoPrompt([Localizable(true)] string title, [Localizable(true)] string message, PromptType type);

    public delegate PromptResultYesNoCancel YesNoCancelPrompt([Localizable(true)] string title, [Localizable(true)] string message, PromptType type);

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
        Error,
        Question
    }
}