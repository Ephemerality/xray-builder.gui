using System;
using System.Collections;
using System.Collections.Generic;
using SQLite;

namespace XRayBuilder.Core.Database
{
    public sealed record DatabaseConfig(string Filename);

    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly SQLiteConnection _sqLiteConnection;

        public DatabaseConnection(DatabaseConfig config)
        {
            _sqLiteConnection = new SQLiteConnection(config.Filename);
        }

        public T FindWithQuery<T>(string query, params object[] args) where T : new() => _sqLiteConnection.FindWithQuery<T>(query, args);
        public int Execute(string query, params object[] args) => _sqLiteConnection.Execute(query, args);
        public List<T> Query<T>(string query, params object[] args) where T : new() => _sqLiteConnection.Query<T>(query, args);
        public int UpdateAll(IEnumerable objects, bool runInTransaction = true) => _sqLiteConnection.UpdateAll(objects, runInTransaction);
        public int Update(object obj) => _sqLiteConnection.Update(obj);
        public int Insert(object obj) => _sqLiteConnection.Insert(obj);
        public int InsertAll(IEnumerable objects, bool runInTransaction = true) => _sqLiteConnection.InsertAll(objects, runInTransaction);
        public int InsertOrReplace(object obj) => _sqLiteConnection.InsertOrReplace(obj);
        public T Get<T>(long primaryKey) where T : new() => _sqLiteConnection.Get<T>(primaryKey);
        public void Delete<T>(long primaryKey) => _sqLiteConnection.Delete<T>(primaryKey);
        public TableQuery<T> Table<T>() where T : new() => _sqLiteConnection.Table<T>();
        public void RunInTransaction(Action action) => _sqLiteConnection.RunInTransaction(action);

        public void Dispose()
        {
            _sqLiteConnection?.Dispose();
        }
    }
}