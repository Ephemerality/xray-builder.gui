using System.Globalization;

namespace XRayBuilder.Core.Libraries.Language.Localization.Languages
{
    public sealed class LanguageNorwegianBokmål : ILanguage
    {
        public LanguageFactory.Enum Language { get; } = LanguageFactory.Enum.NorwegianBokmål;
        public string Label { get; } = "Norsk Bokmål (nb-NO)";
        public CultureInfo CultureInfo { get; } = CultureInfo.GetCultureInfo("nb-NO");
    }
}