using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using XRayBuilder.Core.Libraries.Parsing.Regex;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Libraries.Database.Logic
{
    public class Identifier
    {
        private static readonly Regex RegexGoodreadsId = new(@"/book/show/(?<id>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex RegexLibraryThingId = new(@"/work/(?<id>[0-9]+)", RegexOptions.Compiled);

        private static string ParseGoodreadsId(string input)
        {
            return RegexGoodreadsId.MatchOrNull(input)?.Groups["id"].Value;
        }

        private static string ParseLibraryThingId(string input)
        {
            return RegexLibraryThingId.MatchOrNull(input)?.Groups["id"].Value;
        }

        private static int GetId(string asin)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT id FROM identifiers WHERE asin = @asin"
            };
            cmd.Parameters.AddWithValue("@asin", asin);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            return Convert.ToInt32(result);
        }

        private static void Update(BookInfo book, int bookId)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = $@"UPDATE identifiers
                    SET book = @book, asin = @asin, goodreads = @goodreads, isbn = @isbn, librarything = @librarything, shelfari = @shelfari
                    WHERE id = '{bookId}'"
            };

            cmd.Parameters.AddWithValue("@book", bookId);
            cmd.Parameters.AddWithValue("@asin", book.Asin);
            cmd.Parameters.AddWithValue("@goodreads", ParseGoodreadsId(book.DataUrl));
            cmd.Parameters.AddWithValue("@isbn", "");
            cmd.Parameters.AddWithValue("@librarything", ParseLibraryThingId(book.DataUrl));
            cmd.Parameters.AddWithValue("@shelfari", "");
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static void Insert(BookInfo book, int bookId)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"INSERT INTO identifiers 
                    (book, asin, goodreads, isbn, librarything, shelfari)
                    VALUES
                    (@book, @asin, @goodreads, @isbn, @librarything, @shelfari)"
            };

            cmd.Parameters.AddWithValue("@book", bookId);
            cmd.Parameters.AddWithValue("@asin", book.Asin);
            cmd.Parameters.AddWithValue("@goodreads", ParseGoodreadsId(book.DataUrl));
            cmd.Parameters.AddWithValue("@isbn", "");
            cmd.Parameters.AddWithValue("@librarything", ParseLibraryThingId(book.DataUrl));
            cmd.Parameters.AddWithValue("@shelfari", "");

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        public static void Add(BookInfo book, int id)
        {
            var bookId = GetId(book.Asin);
            if (bookId == 0)
                Insert(book, id);
            else
                Update(book, id);
        }

        public static void Add(BookInfo book)
        {
            var bookId = GetId(book.Asin);
            if (bookId == 0)
                Insert(book, bookId);
            else
                Update(book, bookId);
        }
    }
}