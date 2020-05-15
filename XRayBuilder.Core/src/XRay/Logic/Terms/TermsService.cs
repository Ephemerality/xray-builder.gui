using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using XRayBuilder.Core.DataSources.Secondary;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.Libraries.Serialization.Xml.Util;
using XRayBuilder.Core.XRay.Artifacts;

namespace XRayBuilder.Core.XRay.Logic.Terms
{
    public class TermsService : ITermsService
    {
        // todo extractor factory
        /// <summary>
        /// Extract terms from the given db.
        /// </summary>
        /// <param name="xrayDb">Connection to any db containing the proper dataset.</param>
        /// <param name="singleUse">If set, will close the connection when complete.</param>
        public IEnumerable<Term> ExtractTermsNew(DbConnection xrayDb, bool singleUse)
        {
            if (xrayDb.State != ConnectionState.Open)
                xrayDb.Open();

            using var command = xrayDb.CreateCommand();
            command.CommandText = "SELECT entity.id,entity.label,entity.type,entity.count,entity_description.text,string.text as sourcetxt FROM entity"
                                  + " LEFT JOIN entity_description ON entity.id = entity_description.entity"
                                  + " LEFT JOIN source ON entity_description.source = source.id"
                                  + " LEFT JOIN string ON source.label = string.id AND string.language = 'en'"
                                  + " WHERE entity.has_info_card = '1'";
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var type = Convert.ToInt32(reader["type"]);

                var newTerm = new Term
                {
                    Id = Convert.ToInt32(reader["id"]),
                    TermName = (string)reader["label"],
                    Type = type == 1 ? "character" : "topic",
                    Desc = (string)reader["text"],
                    DescSrc = reader["sourcetxt"] == DBNull.Value ? "" : (string)reader["sourcetxt"],
                    MatchCase = type == 1 // characters should match case by default
                };

                // Real locations aren't needed for extracting terms for preview or XML saving, but need count
                var i = Convert.ToInt32(reader["count"]);
                for (; i > 0; i--)
                    newTerm.Locs.Add(null);

                // TODO: Should probably also confirm whether this URL exists or not
                if (newTerm.DescSrc == "Wikipedia")
                    newTerm.DescUrl = $@"http://en.wikipedia.org/wiki/{newTerm.TermName.Replace(" ", "_")}";

                yield return newTerm;
            }

            if (singleUse)
                xrayDb.Close();
        }

        /// <summary>
        /// Extract terms from the old JSON X-Ray format
        /// </summary>
        public IEnumerable<Term> ExtractTermsOld(string path)
        {
            string readContents;
            using (var streamReader = new StreamReader(path, Encoding.UTF8))
                readContents = streamReader.ReadToEnd();

            var xray = JObject.Parse(readContents);
            return xray["terms"] == null
                ? Enumerable.Empty<Term>()
                : xray["terms"].Children().Select(token => token.ToObject<Term>());
        }

        /// <summary>
        /// Downloads terms from the <paramref name="dataSource"/> and saves them to <paramref name="outFile"/>
        /// </summary>
        public async Task DownloadAndSaveAsync(ISecondarySource dataSource, string dataUrl, string outFile, string asin, string tld, bool includeTopics, IProgressBar progress, CancellationToken token = default)
        {
            var terms = (await dataSource.GetTermsAsync(dataUrl, asin, tld, includeTopics, progress, token)).ToArray();
            if (terms.Length == 0)
                throw new Exception($"No terms were found on {dataSource.Name}");
            XmlUtil.SerializeToFile(terms, outFile);
        }

        public IEnumerable<Term> ReadTermsFromTxt(string txtFile)
        {
            using var streamReader = new StreamReader(txtFile, Encoding.UTF8);
            var termId = 1;
            var lineCount = 1;
            while (!streamReader.EndOfStream)
            {
                var type = streamReader.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(type))
                    continue;
                lineCount++;
                if (type != "character" && type != "topic")
                    throw new Exception($"Error: Invalid term type \"{type}\" on line {lineCount}");

                yield return new Term
                {
                    Type = type,
                    TermName = streamReader.ReadLine(),
                    Desc = streamReader.ReadLine(),
                    MatchCase = type == "character",
                    DescSrc = "shelfari",
                    Id = termId++
                };

                lineCount += 2;
            }
        }
    }
}