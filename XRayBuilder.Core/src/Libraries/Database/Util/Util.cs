using System;
using System.Data.SQLite;

namespace XRayBuilder.Core.Libraries.Database.Util
{
    public class Util
    {
        public static int GetBookId(string asin)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT * FROM books WHERE asin = @asin"
            };
            cmd.Parameters.AddWithValue("@asin", asin);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            return Convert.ToInt32(result);
        }

        public static int GetAuthorId(string name)
        {
            using var con = Database.Open();
            using var cmd = new SQLiteCommand(con)
            {
                CommandText = @"SELECT * FROM authors WHERE name = @name"
            };
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Prepare();

            var result = cmd.ExecuteScalar();
            result = result == DBNull.Value ? null : result;
            return Convert.ToInt32(result);
        }
    }
}