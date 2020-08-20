using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XRayBuilder.Core.Libraries.Logging;
using XRayBuilder.Core.Libraries.Progress;
using XRayBuilder.Core.XRay.Model;

namespace XRayBuilder.Core.XRay.Logic.Export
{
    public sealed class XRayExporterSqlite : IXRayExporter
    {
        private readonly ILogger _logger;

        public XRayExporterSqlite(ILogger logger)
        {
            _logger = logger;
        }

        public void Export(XRay xray, string path, IProgressBar progress, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.Log("Building new X-Ray database...");
                using var database = Create(path);
                _logger.Log("Done building initial database. Populating with info from source X-Ray...");
                Populate(xray, database, progress, cancellationToken);
                _logger.Log("Updating indices...");
                UpdateIndices(database);
                database.Close();
            }
            finally
            {
                // Force a garbage collection so sqlite releases the file
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        /// <summary>
        /// Creates an empty database at <paramref name="path"/> and returns the open database connection
        /// </summary>
        /// <exception cref="IOException"></exception>
        private SQLiteConnection Create(string path)
        {
            SQLiteConnection.CreateFile(path);
            var db = new SQLiteConnection($"Data Source={path};Version=3;");
            db.Open();
            string sql;
            try
            {
                using var streamReader = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\dist\BaseDB.sql", Encoding.UTF8);
                sql = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while opening the BaseDB.sql file. Ensure you extracted it to the same directory as the program.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
            var command = new SQLiteCommand("BEGIN; " + sql + " COMMIT;", db);
            command.ExecuteNonQuery();
            command.Dispose();
            command = new SQLiteCommand("PRAGMA user_version = 1; PRAGMA encoding = utf8; BEGIN;", db);
            command.ExecuteNonQuery();
            command.Dispose();

            return db;
        }

        /// <summary>
        /// Create indices for optimization
        /// </summary>
        private void UpdateIndices(SQLiteConnection db)
        {
            const string sql = "CREATE INDEX idx_occurrence_start ON occurrence(start ASC);\n"
                               + "CREATE INDEX idx_entity_type ON entity(type ASC);\n"
                               + "CREATE INDEX idx_entity_excerpt ON entity_excerpt(entity ASC); COMMIT;";
            using var command = new SQLiteCommand(sql, db);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        /// <summary>
        /// Populates a <paramref name="db"/> with the data from <paramref name="xray"/>
        /// </summary>
        private void Populate(XRay xray, SQLiteConnection db, IProgressBar progress, CancellationToken token = default)
        {
            var sql = new StringBuilder(xray.Terms.Count * 256);
            var personCount = 0;
            var termCount = 0;
            var command = new SQLiteCommand($"update string set text='{xray.DataUrl}' where id=15", db);
            command.ExecuteNonQuery();

            _logger.Log("Updating database with terms, descriptions, and excerpts...");
            //Write all entities and occurrences
            _logger.Log($"Writing {xray.Terms.Count} terms...");
            progress?.Set(0, xray.Terms.Count);
            command = new SQLiteCommand("insert into entity (id, label, loc_label, type, count, has_info_card) values (@id, @label, null, @type, @count, 1)", db);
            var command2 = new SQLiteCommand("insert into entity_description (text, source_wildcard, source, entity) values (@text, @source_wildcard, @source, @entity)", db);
            var command3 = new SQLiteCommand("insert into occurrence (entity, start, length) values (@entity, @start, @length)", db);
            foreach (var t in xray.Terms)
            {
                token.ThrowIfCancellationRequested();
                if (t.Type == "character") personCount++;
                else if (t.Type == "topic") termCount++;
                command.Parameters.Add("@id", DbType.Int32).Value = t.Id;
                command.Parameters.Add("@label", DbType.String).Value = t.TermName;
                command.Parameters.Add("@type", DbType.Int32).Value = t.Type == "character" ? 1 : 2;
                command.Parameters.Add("@count", DbType.Int32).Value = t.Occurrences.Count;
                command.ExecuteNonQuery();

                command2.Parameters.Add("@text", DbType.String).Value = string.IsNullOrEmpty(t.Desc) ? "No description available." : t.Desc;
                command2.Parameters.Add("@source_wildcard", DbType.String).Value = t.TermName;
                command2.Parameters.Add("@source", DbType.Int32).Value = t.DescSrc == "shelfari" ? 4 : 6;
                command2.Parameters.Add("@entity", DbType.Int32).Value = t.Id;
                command2.ExecuteNonQuery();

                foreach (var loc in t.Occurrences)
                {
                    command3.Parameters.Add("@entity", DbType.Int32).Value = t.Id;
                    command3.Parameters.Add("@start", DbType.Int32).Value = loc[0];
                    command3.Parameters.Add("@length", DbType.Int32).Value = loc[1];
                    command3.ExecuteNonQuery();
                }
                progress?.Add(1);
            }

            //Write excerpts and entity_excerpt table
            _logger.Log($"Writing {xray.Excerpts.Count} excerpts...");
            command.CommandText = "insert into excerpt (id, start, length, image, related_entities, goto) values (@id, @start, @length, @image, @rel_ent, null);";
            command.Parameters.Clear();
            command2.CommandText = "insert into entity_excerpt (entity, excerpt) values (@entityId, @excerptId)";
            command2.Parameters.Clear();
            progress?.Set(0, xray.Excerpts.Count);
            foreach (var e in xray.Excerpts)
            {
                token.ThrowIfCancellationRequested();
                command.Parameters.Add("id", DbType.Int32).Value = e.Id;
                command.Parameters.Add("start", DbType.Int32).Value = e.Start;
                command.Parameters.Add("length", DbType.Int32).Value = e.Length;
                command.Parameters.Add("image", DbType.String).Value = e.Image;
                command.Parameters.Add("rel_ent", DbType.String).Value = string.Join(",", e.RelatedEntities.Where(en => en != 0).ToArray()); // don't write 0 (notable flag)
                command.ExecuteNonQuery();
                foreach (var ent in e.RelatedEntities)
                {
                    token.ThrowIfCancellationRequested();
                    if (ent == 0) continue; // skip notable flag
                    command2.Parameters.Add("@entityId", DbType.Int32).Value = ent;
                    command2.Parameters.Add("@excerptId", DbType.Int32).Value = e.Id;
                    command2.ExecuteNonQuery();
                }
                progress?.Add(1);
            }

            // create links to notable clips in order of popularity
            _logger.Log("Adding notable clips...");
            command.Parameters.Clear();
            var notablesOnly = xray.Excerpts.Where(ex => ex.Notable).OrderByDescending(ex => ex.Highlights);
            foreach (var notable in notablesOnly)
            {
                command.CommandText = $"insert into entity_excerpt (entity, excerpt) values (0, {notable.Id})";
                command.ExecuteNonQuery();
            }

            // Populate some more clips if not enough were found initially
            // TODO: Add a config value in settings for this amount
            var toAdd = new List<Excerpt>(20);
            if (xray.FoundNotables <= 20 && xray.FoundNotables + xray.Excerpts.Count <= 20)
                toAdd.AddRange(xray.Excerpts);
            else if (xray.FoundNotables <= 20)
            {
                var rand = new Random();
                var eligible = xray.Excerpts.Where(ex => !ex.Notable).ToList();
                while (xray.FoundNotables <= 20 && eligible.Count > 0)
                {
                    var randEx = eligible.ElementAt(rand.Next(eligible.Count));
                    toAdd.Add(randEx);
                    eligible.Remove(randEx);
                    xray.FoundNotables++;
                }
            }
            foreach (var excerpt in toAdd)
            {
                command.CommandText = $"insert into entity_excerpt (entity, excerpt) values (0, {excerpt.Id})";
                command.ExecuteNonQuery();
            }
            command.Dispose();

            token.ThrowIfCancellationRequested();
            _logger.Log("Writing top mentions...");
            var sorted =
                xray.Terms.Where(t => t.Type.Equals("character"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.Clear();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=1;\n",
                string.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            sorted =
                xray.Terms.Where(t => t.Type.Equals("topic"))
                    .OrderByDescending(t => t.Locs.Count)
                    .Select(t => t.Id)
                    .ToList();
            sql.AppendFormat("update type set top_mentioned_entities='{0}' where id=2;",
                string.Join(",", sorted.GetRange(0, Math.Min(10, sorted.Count))));
            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();

            token.ThrowIfCancellationRequested();
            _logger.Log("Writing metadata...");

            sql.Clear();
            sql.AppendFormat(
                "insert into book_metadata (srl, erl, has_images, has_excerpts, show_spoilers_default, num_people, num_terms, num_images, preview_images) "
                + "values ({0}, {1}, 0, 1, 0, {2}, {3}, 0, null);", xray.Srl, xray.Erl, personCount, termCount);

            command = new SQLiteCommand(sql.ToString(), db);
            command.ExecuteNonQuery();
            command.Dispose();
        }
    }
}