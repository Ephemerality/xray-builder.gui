using System;
using System.Data.SQLite;
using XRayBuilder.Core.Model;

namespace XRayBuilder.Core.Libraries.Database.Logic
{
    public class Book
    {
        public static void Add(BookInfo book, string path)
        {
            var bookId = GetId(book.Asin);
            if (bookId == 0)
                Insert(book, path);
            else
                Update(book, path, bookId);

            bookId = GetId(book.Asin);

            Identifier.Add(book, bookId);

            if (!string.IsNullOrEmpty(book.Author))
                Author.Add(book, bookId);
        }

        private static void Insert(BookInfo book, string path)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"INSERT INTO books
                    (title, author, asin, goodreads, isbn, url, path)
                    VALUES
                    (@title, @author, @asin, @goodreads, @isbn, @url, @path)"
            };

            cmd.Parameters.AddWithValue("@title", book.Title);
            cmd.Parameters.AddWithValue("@author", book.Author);
            cmd.Parameters.AddWithValue("@asin", book.Asin);
            cmd.Parameters.AddWithValue("@goodreads", book.DataUrl);
            cmd.Parameters.AddWithValue("@isbn", "");
            cmd.Parameters.AddWithValue("@url", book.AmazonUrl);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static void Update(BookInfo book, string path, int id)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @$"UPDATE books
                    SET title = @title, author = @author, asin = @asin, goodreads = @goodreads, isbn = @isbn, url = @url, path = @path
                    WHERE id = '{id}'"
            };

            cmd.Parameters.AddWithValue("@title", book.Title);
            cmd.Parameters.AddWithValue("@author", book.Author);
            cmd.Parameters.AddWithValue("@asin", book.Asin);
            cmd.Parameters.AddWithValue("@goodreads", book.DataUrl);
            cmd.Parameters.AddWithValue("@isbn", "");
            cmd.Parameters.AddWithValue("@url", book.AmazonUrl);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static int GetId(string asin)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT id FROM books WHERE asin = @asin"
            };
            cmd.Parameters.AddWithValue("@asin", asin);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            return Convert.ToInt32(result);
        }
    }
}