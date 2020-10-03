using System.Globalization;

namespace XRayBuilder.Core.Libraries.Language.Localization
{
    public interface ILanguage
    {
        LanguageFactory.Enum Language { get; }
        string Label { get; }
        CultureInfo CultureInfo { get; }
    }
}