using System.Text.RegularExpressions;

namespace XRayBuilder.Core.Libraries.Parsing.Regex
{
    public static class RegexExtensions
    {
        public static Match MatchOrNull(this System.Text.RegularExpressions.Regex regex, string input)
        {
            if (input == null)
                return null;

            var match = regex.Match(input);
            return match.Success ? match : null;
        }
    }
}