using System;
using System.Data.SQLite;
using System.IO;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Libraries.Database.Logic;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Libraries.Database
{
    public class Database
    {
        public static string DatabaseFile = Environment.CurrentDirectory + @"\XRayBuilder.Database.db";

        public Database()
        {
            try
            {
                if (File.Exists(DatabaseFile)) return;
                SQLiteConnection.CreateFile(DatabaseFile);
                CreateTables();
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException(ex.Message);
            }
        }

        public static string DataSource => $"data source={DatabaseFile};Version=3;";

        private static void CreateTables()
        {
            using var con = new SQLiteConnection($"{DataSource}");
            con.Open();

            using var cmd = new SQLiteCommand(con);
            using (var tran = con.BeginTransaction())
            {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS books(
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL DEFAULT '' COLLATE NOCASE,
                    author TEXT COLLATE NOCASE,
                    asin TEXT DEFAULT '' COLLATE NOCASE,
                    goodreads TEXT DEFAULT '' COLLATE NOCASE,
                    isbn TEXT DEFAULT '' COLLATE NOCASE,
                    url TEXT DEFAULT '' COLLATE NOCASE,
                    path TEXT NOT NULL DEFAULT '',
                    UNIQUE(id, asin))";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS authors(
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL COLLATE NOCASE,
                    asin TEXT DEFAULT '' COLLATE NOCASE,
                    biography TEXT DEFAULT '' COLLATE NOCASE,
                    url TEXT DEFAULT '' COLLATE NOCASE,
                    UNIQUE(id, asin))";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS books_authors_link (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    book INTEGER NOT NULL,
                    author INTEGER NOT NULL)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS identifiers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    book INTEGER NOT NULL,
                    asin TEXT DEFAULT '' COLLATE NOCASE,
                    goodreads TEXT DEFAULT '' COLLATE NOCASE,
                    isbn TEXT DEFAULT '' COLLATE NOCASE,
                    librarything TEXT DEFAULT '' COLLATE NOCASE,
                    shelfari TEXT DEFAULT '' COLLATE NOCASE,
                    UNIQUE(asin))";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"PRAGMA user_version = 1";
                cmd.ExecuteNonQuery();

                tran.Commit();
            }

            cmd.Dispose();
            con.Close();
        }

        public static SQLiteConnection Open()
        {
            try
            {
                var con = new SQLiteConnection($"{DataSource}");
                con.Open();
                return con;
            }

            catch (SQLiteException ex)
            {
                throw new SQLiteException(ex.Message);
            }
        }

        public void AddBook(BookInfo book, string path)
        {
            Book.Add(book, path);
        }

        public void AddAuthor(AuthorSearchResults author)
        {
            Author.Add(author);
        }

        public void AddIdentifier(BookInfo book)
        {
            Identifier.Add(book);
        }
    }
}