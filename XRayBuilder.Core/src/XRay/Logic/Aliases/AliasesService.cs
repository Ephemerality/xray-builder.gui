using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.XRay.Logic.Aliases
{
    public sealed class AliasesService : IAliasesService
    {
        #region CommonTitles
        private readonly string[] _commonTitles = { "Mr", "Mrs", "Ms", "Miss", "Dr", "Herr", "Monsieur", "Hr", "Frau",
            "A V M", "Admiraal", "Admiral", "Alderman", "Alhaji", "Ambassador", "Baron", "Barones", "Brig",
            "Brigadier", "Brother", "Canon", "Capt", "Captain", "Cardinal", "Cdr", "Chief", "Cik", "Cmdr", "Col",
            "Colonel", "Commandant", "Commander", "Commissioner", "Commodore", "Comte", "Comtessa", "Congressman",
            "Conseiller", "Consul", "Conte", "Contessa", "Corporal", "Councillor", "Count", "Countess", "Air Cdre",
            "Air Commodore", "Air Marshal", "Air Vice Marshal", "Brig Gen", "Brig General", "Brigadier General",
            "Crown Prince", "Crown Princess", "Dame", "Datin", "Dato", "Datuk", "Datuk Seri", "Deacon", "Deaconess",
            "Dean", "Dhr", "Dipl Ing", "Doctor", "Dott", "Dott Sa", "Dr", "Dr Ing", "Dra", "Drs", "Embajador",
            "Embajadora", "En", "Encik", "Eng", "Eur Ing", "Exma Sra", "Exmo Sr", "Father", "First Lieutient",
            "First Officer", "Flt Lieut", "Flying Officer", "Fr", "Frau", "Fraulein", "Fru", "Gen", "Generaal",
            "General", "Governor", "Graaf", "Gravin", "Group Captain", "Grp Capt", "H E Dr", "H H", "H M", "H R H",
            "Hajah", "Haji", "Hajim", "Her Highness", "Her Majesty", "Herr", "High Chief", "His Highness",
            "His Holiness", "His Majesty", "Hon", "Hr", "Hra", "Ing", "Ir", "Jonkheer", "Judge", "Justice",
            "Khun Ying", "Kolonel", "Lady", "Lcda", "Lic", "Lieut", "Lieut Cdr", "Lieut Col", "Lieut Gen", "Lord",
            "Madame", "Mademoiselle", "Maj Gen", "Major", "Master", "Mevrouw", "Miss", "Mlle", "Mme", "Monsieur",
            "Monsignor", "Mstr", "Nti", "Pastor", "President", "Prince", "Princess", "Princesse", "Prinses", "Prof",
            "Prof Dr", "Prof Sir", "Professor", "Puan", "Puan Sri", "Rabbi", "Rear Admiral", "Rev", "Rev Canon",
            "Rev Dr", "Rev Mother", "Reverend", "Rva", "Senator", "Sergeant", "Sheikh", "Sheikha", "Sig", "Sig Na",
            "Sig Ra", "Sir", "Sister", "Sqn Ldr", "Sr", "Sr D", "Sra", "Srta", "Sultan", "Tan Sri", "Tan Sri Dato",
            "Tengku", "Teuku", "Than Puying", "The Hon Dr", "The Hon Justice", "The Hon Miss", "The Hon Mr",
            "The Hon Mrs", "The Hon Ms", "The Hon Sir", "The Very Rev", "Toh Puan", "Tun", "Vice Admiral",
            "Viscount", "Viscountess", "Wg Cdr", "Jr", "Sr", "Sheriff", "Special Agent", "Detective", "Lt" };
        #endregion

        private readonly Regex _sanitizePattern;

        public AliasesService(ILogger logger)
        {
            //Try to load custom common titles from BaseSplitIgnore.txt
            try
            {
                using var streamReader = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\dist\BaseSplitIgnore.txt", Encoding.UTF8);
                var customSplitIgnore = streamReader.ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.None)
                    .Where(r => !r.StartsWith("//")).ToArray();
                if (customSplitIgnore.Length >= 1)
                    _commonTitles = customSplitIgnore;
                logger.Log("Splitting aliases using custom common titles file...");
            }
            catch (Exception ex)
            {
                logger.Log("An error occurred while opening the BaseSplitIgnore.txt file.\r\n" +
                    "Ensure you extracted it to the same directory as the program.\r\n" +
                    ex.Message + "\r\nUsing built-in default terms...");
            }

            _sanitizePattern = new Regex($@"( ?({string.Join("|", _commonTitles)})\.? )|(^[A-Z]\. )|( [A-Z]\.)|("")|(“)|(”)|(,)|(')");
        }

        public IEnumerable<KeyValuePair<string, string[]>> GenerateAliases(IEnumerable<Term> characters)
        {
            var aliasesByTermName = new Dictionary<string, string[]>();

            foreach (var character in characters)
            {
                // Short-circuit if no alias is possible, if this isn't a character, or if aliases already exist (manually added or pre-built)
                if (!character.TermName.Contains(" ") || character.Type != "character" || character.Aliases.Count > 0)
                {
                    aliasesByTermName.Add(character.TermName, character.Aliases.ToArray());
                    continue;
                }

                // Filter out any already-existing aliases from other terms
                var dedupedAliases = GenerateAliasesForTerm(character)
                    .Where(alias => !aliasesByTermName.Values.SelectMany(existingAliases => existingAliases).Contains(alias))
                    .ToArray();

                if (dedupedAliases.Length > 0)
                    aliasesByTermName.Add(character.TermName, dedupedAliases);
            }

            return aliasesByTermName;
        }

        private IEnumerable<string> GenerateAliasesForTerm(Term term)
        {
            var aliases = new List<string>();
            var textInfo = new CultureInfo("en-US", false).TextInfo;

            var sanitizedName = _sanitizePattern.Replace(term.TermName, string.Empty);
            if (sanitizedName != term.TermName)
                aliases.Add(sanitizedName);

            sanitizedName = Regex.Replace(sanitizedName, @"\s+", " ");
            sanitizedName = Regex.Replace(sanitizedName, @"( ?V?I{0,3}$)", string.Empty);
            sanitizedName = Regex.Replace(sanitizedName, @"(\(aka )", "(");

            var bracketedName = Regex.Match(sanitizedName, @"(.*)(\()(.*)(\))");
            if (bracketedName.Success)
            {
                aliases.Add(bracketedName.Groups[3].Value);
                aliases.Add(bracketedName.Groups[1].Value.TrimEnd());
                sanitizedName = sanitizedName.Replace(bracketedName.Groups[2].Value, "")
                    .Replace(bracketedName.Groups[4].Value, "");
            }

            if (sanitizedName.Contains(" "))
            {
                sanitizedName = sanitizedName.Replace(" &amp;", "").Replace(" &", "");
                var words = sanitizedName.Split(' ');
                string FixAllCaps(string word) => word.ToUpper() == word ? textInfo.ToTitleCase(word.ToLower()) : word;
                aliases.AddRange(words.Select(FixAllCaps));
            }

            // also remove any entries that are also in the ignore list
            return aliases.Where(alias => !_commonTitles.Contains(alias));
        }
    }
}