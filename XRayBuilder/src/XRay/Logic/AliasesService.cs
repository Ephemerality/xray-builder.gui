using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XRayBuilderGUI.Libraries.Logging;

namespace XRayBuilderGUI.XRay.Logic
{
    public sealed class AliasesService : IAliasesService
    {
        private readonly ILogger _logger;

        public AliasesService(ILogger logger)
        {
            _logger = logger;
        }

        // todo make this better
        public void LoadAliasesForXRay(XRay xray)
        {
            var aliasesByTermName = LoadAliasesFromFile(xray.AliasPath);
            if (aliasesByTermName == null)
                return;

            for (var i = 0; i < xray.Terms.Count; i++)
            {
                var t = xray.Terms[i];
                if (aliasesByTermName.ContainsKey(t.TermName))
                {
                    if (t.Aliases.Count > 0)
                    {
                        // If aliases exist (loaded from Goodreads), remove any duplicates and add them in the order from the aliases file
                        // Otherwise, the website would take precedence and that could be bad?
                        foreach (var alias in aliasesByTermName[t.TermName])
                        {
                            if (t.Aliases.Contains(alias))
                            {
                                t.Aliases.Remove(alias);
                                t.Aliases.Add(alias);
                            }
                            else
                                t.Aliases.Add(alias);
                        }
                    }
                    else
                        t.Aliases = new List<string>(aliasesByTermName[t.TermName]);
                    // If first alias is "/c", character searches will be case-sensitive
                    // If it is /d, delete this character
                    // If /n, will not match excerpts but will leave character in X-Ray
                    // If /r, character's aliases (and ONLY the aliases) will be processed as Regular Expressions (case-sensitive unless specified in regex)
                    if (t.Aliases[0] == "/c")
                    {
                        t.MatchCase = true;
                        t.Aliases.Remove("/c");
                    }
                    else if (t.Aliases[0] == "/d")
                    {
                        xray.Terms.Remove(t);
                        i--;
                    }
                    else if (t.Aliases[0] == "/n")
                    {
                        t.Match = false;
                        t.Aliases.Remove("/n");
                    }
                    else if (t.Aliases[0] == "/r")
                    {
                        t.RegexAliases = true;
                        t.Aliases.Remove("/r");
                    }
                }
            }
        }

        public Dictionary<string, string[]> LoadAliasesFromFile(string aliasFile)
        {
            if (!File.Exists(aliasFile))
                return null;

            var aliasesByTermName = new Dictionary<string, string[]>();
            using var streamReader = new StreamReader(aliasFile, Encoding.UTF8);
            while (!streamReader.EndOfStream)
            {
                var input = streamReader.ReadLine();
                var temp = input?.Split('|');
                if (temp == null || temp.Length <= 1 || temp[0] == "" || temp[0].StartsWith("#"))
                    continue;
                var temp2 = input.Substring(input.IndexOf('|') + 1).Split(',');
                //Check for misplaced pipe character in aliases
                if (temp2[0] != "" && temp2.Any(r => Regex.Match(@"\|", r).Success))
                {
                    _logger.Log("An error occurred parsing the alias file. Ignoring term: " + temp[0] + " aliases.\r\nCheck the file is in the correct format: Character Name|Alias1,Alias2,Etc");
                    continue;
                }
                if (temp2.Length == 0 || temp2[0] == "") continue;
                if (aliasesByTermName.ContainsKey(temp[0]))
                    _logger.Log("Duplicate alias of " + temp[0] + " found. Ignoring the duplicate.");
                else
                    aliasesByTermName.Add(temp[0], temp2);
            }

            return aliasesByTermName;
        }
    }
}
