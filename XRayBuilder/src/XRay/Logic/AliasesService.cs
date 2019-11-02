using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XRayBuilderGUI.Libraries.Logging;

namespace XRayBuilderGUI.XRay.Logic
{
    public class AliasesService : IAliasesService
    {
        private readonly ILogger _logger;

        public AliasesService(ILogger logger)
        {
            _logger = logger;
        }

        public Dictionary<string, string[]> LoadAliases(string aliasFile)
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
