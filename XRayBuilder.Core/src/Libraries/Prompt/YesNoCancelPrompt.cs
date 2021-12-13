using System.ComponentModel;

namespace XRayBuilder.Core.Libraries.Prompt
{
    public delegate PromptResultYesNoCancel YesNoCancelPrompt([Localizable(true)] string title, [Localizable(true)] string message, PromptType type);
}