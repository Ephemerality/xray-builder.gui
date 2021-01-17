using System;
using System.Data.SQLite;
using XRayBuilder.Core.DataSources.Amazon;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Libraries.Database.Logic
{
    public class Author
    {
        public static void Add(BookInfo book, int bookId)
        {
            var authorId = GetId(book.Author);
            if (authorId == 0)
                Insert(book);
            else
                Update(book, authorId);

            authorId = GetId(book.Author);
            if (authorId != 0)
                Link.Add(authorId, bookId);
        }

        public static void Add(AuthorSearchResults author)
        {
            var authorId = GetId(author.Name);
            if (authorId == 0)
                Insert(author);
            else
                Update(author, authorId);
        }

        private static void Insert(AuthorSearchResults author)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText =
                    @"INSERT INTO authors
                    (name, asin, biography, url)
                    VALUES
                    (@name, @asin, @biography, @url)"
            };

            cmd.Parameters.AddWithValue("@name", author.Name);
            cmd.Parameters.AddWithValue("@asin", author.Asin);
            cmd.Parameters.AddWithValue("@biography", author.Biography);
            cmd.Parameters.AddWithValue("@url", author.Url);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static void Update(AuthorSearchResults author, int id)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText =
                    $@"UPDATE authors
                   SET name = @name, asin = @asin, biography = @biography, url = @url
                   WHERE id = '{id}'"
            };

            cmd.Parameters.AddWithValue("@name", author.Name);
            cmd.Parameters.AddWithValue("@asin", author.Asin);
            cmd.Parameters.AddWithValue("@biography", author.Biography);
            cmd.Parameters.AddWithValue("@url", author.Url);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static void Insert(BookInfo book)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText =
                    @"INSERT INTO authors
                    (name, asin, biography, url)
                    VALUES
                    (@name, @asin, @biography, @url)"
            };

            cmd.Parameters.AddWithValue("@name", book.Author);
            cmd.Parameters.AddWithValue("@asin", book.AuthorAsin);
            cmd.Parameters.AddWithValue("@biography", "");
            cmd.Parameters.AddWithValue("@url", "");

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static void Update(BookInfo book, int id)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText =
                    $@"UPDATE authors
                   SET name = @name, asin = @asin, biography = @biography, url = @url
                   WHERE id = '{id}'"
            };

            cmd.Parameters.AddWithValue("@name", book.Author);
            cmd.Parameters.AddWithValue("@asin", book.AuthorAsin);
            cmd.Parameters.AddWithValue("@biography", "");
            cmd.Parameters.AddWithValue("@url", "");

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static int GetId(string name)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT id FROM authors WHERE name = @name"
            };
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            return Convert.ToInt32(result);
        }
    }
}