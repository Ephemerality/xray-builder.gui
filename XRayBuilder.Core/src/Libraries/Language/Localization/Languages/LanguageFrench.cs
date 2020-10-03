using System.Globalization;

namespace XRayBuilder.Core.Libraries.Language.Localization.Languages
{
    public sealed class LanguageFrench : ILanguage
    {
        public LanguageFactory.Enum Language { get; } = LanguageFactory.Enum.French;
        public string Label { get; } = "Français (fr)";
        public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfo("fr");
    }
}