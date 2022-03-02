using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace XRayBuilder.Core.Libraries.Language.Localization
{
    public sealed class LanguageFactory : Factory<LanguageFactory.Enum, ILanguage>
    {
        public enum Enum
        {
            English,
            French,
            NorwegianBokmål,
            German
        }

        protected override IReadOnlyDictionary<Enum, ILanguage> Dictionary { get; }

        public LanguageFactory(IEnumerable<ILanguage> languages)
        {
            Dictionary = languages.ToDictionary(language => language.Language);
        }

        /// <summary>
        /// Returns the matching <see cref="ILanguage"/> if <paramref name="language"/> is valid and supported, null otherwise.
        /// </summary>
        [CanBeNull]
        public ILanguage Get(string language)
            => System.Enum.TryParse<Enum>(language, out var languageEnum)
                ? Get(languageEnum)
                : null;

        /// <summary>
        /// Get the current language of the system, if it's supported.
        /// </summary>
        [CanBeNull]
        public ILanguage GetWindowsLanguage()
            => Dictionary.Values.FirstOrDefault(value => value.CultureInfo.Equals(CultureInfo.CurrentUICulture));
    }
}