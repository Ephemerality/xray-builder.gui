#if NETFRAMEWORK
using Pluralize.NET;
#else
using Pluralize.NET.Core;
#endif
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XRayBuilder.Core.Libraries.Primitives.Extensions
{
    public static class StringExtensions
    {
        private static readonly Pluralizer _pluralizer = new Pluralizer();

        //http://stackoverflow.com/questions/444798/case-insensitive-containsstring
        public static bool Contains(this string source, string toCheck, StringComparison comp)
            => source?.IndexOf(toCheck, comp) >= 0;

        public static bool ContainsIgnorecase(this string source, string toCheck)
            => source.Contains(toCheck, StringComparison.OrdinalIgnoreCase);

        public static string Plural(this string value, int count)
        {
            return count == 1
                ? value
                : _pluralizer.Pluralize(value);
        }

        public static int? TryParseInt(this string s)
        {
            return s.TryParseInt(NumberStyles.AllowThousands, CultureInfo.CurrentCulture)
                   ?? s.TryParseInt(NumberStyles.AllowThousands, new CultureInfo("nl-NL"))
                   ?? s.TryParseInt(NumberStyles.AllowThousands, new CultureInfo("en-US"));
        }

        public static int? TryParseInt(this string s, NumberStyles style, IFormatProvider provider)
        {
            return int.TryParse(s, style, provider, out var result)
                ? (int?) result
                : null;
        }

        //http://stackoverflow.com/questions/166855/c-sharp-preg-replace
        public static string PregReplace(this string input, string[] pattern, string[] replacements)
        {
            if (replacements.Length != pattern.Length)
                throw new ArgumentException("Replacement and pattern arrays must be balanced");

            for (var i = 0; i < pattern.Length; i++)
            {
                input = Regex.Replace(input, pattern[i], replacements[i]);
            }
            return input;
        }

        //http://www.levibotelho.com/development/c-remove-diacritics-accents-from-a-string/
        public static string RemoveDiacritics(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) !=
                                        UnicodeCategory.NonSpacingMark).ToArray();

            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        public static string TrimNonPrintableAscii(this string value)
        {
            var pattern = new Regex("[^ -~]+");
            return pattern.Replace(value, "");
        }
    }
}