using System.Globalization;

namespace XRayBuilder.Core.Libraries.Language.Localization.Languages
{
    public sealed class LanguageEnglish : ILanguage
    {
        public LanguageFactory.Enum Language { get; } = LanguageFactory.Enum.English;
        public string Label { get; } = "English (en)";
        public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfo("en");
    }
}