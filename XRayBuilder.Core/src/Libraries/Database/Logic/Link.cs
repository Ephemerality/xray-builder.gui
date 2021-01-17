using System;
using System.Data.SQLite;

namespace XRayBuilder.Core.Libraries.Database.Logic
{
    public class Link
    {
        public static void Add(int authorId, int bookId)
        {
            if (Exists(authorId, bookId)) return;

            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"INSERT INTO books_authors_link
                    (book, author)
                    VALUES
                    (@book, @author)"
            };

            cmd.Parameters.AddWithValue("@author", authorId);
            cmd.Parameters.AddWithValue("@book", bookId);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            con.Close();
        }

        private static bool Exists(int bookId, int authorId)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT id FROM books_authors_link WHERE book = @bookId and author = @authorId"
            };
            cmd.Parameters.AddWithValue("@bookId", bookId);
            cmd.Parameters.AddWithValue("@authorId", authorId);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            var i = Convert.ToInt32(result);
            return i > 0;
        }
    }
}