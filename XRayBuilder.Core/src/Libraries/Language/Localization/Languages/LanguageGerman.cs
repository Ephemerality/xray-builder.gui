using System.Globalization;

namespace XRayBuilder.Core.Libraries.Language.Localization.Languages
{
    public sealed class LanguageGerman : ILanguage
    {
        public LanguageFactory.Enum Language => LanguageFactory.Enum.German;
        public string Label => "Deutsch (de)";
        public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfo("de");
    }
}