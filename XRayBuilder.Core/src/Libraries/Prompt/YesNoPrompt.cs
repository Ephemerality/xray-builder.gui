using System.ComponentModel;

namespace XRayBuilder.Core.Libraries.Prompt
{
    public delegate PromptResultYesNo YesNoPrompt([Localizable(true)] string title, [Localizable(true)] string message, PromptType type);
}