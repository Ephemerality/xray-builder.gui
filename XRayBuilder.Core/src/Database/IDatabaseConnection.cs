using System;
using System.Collections;
using System.Collections.Generic;
using SQLite;

namespace XRayBuilder.Core.Database
{
    /// <summary>
    /// Wrapper to isolate the important methods from <see cref="SQLiteConnection"/>
    /// </summary>

    /** TODO LIST **/
    /*
     * decide if this wrapper is even worthwhile
     * error handling on db init
     * decide best way to upsert models
     * decide where responsiblity lies for updating database
        * db needs to be bootstrapped in Core instead of UI if it will be updated in misc services
            * this would mean Console would need to init the db even if unused
     * if sqlite-net and sqlitepclraw stay, consider whether it's worth migrating the System.Data.Sqlite calls as well
     * if not, make writing repos less tedious and repetitive
     * find best place to assign data source ids from URL (static parse X Id inside the repo w/ regex isn't good)
     */
    public interface IDatabaseConnection
    {
        T FindWithQuery<T>(string query, params object[] args) where T : new();
        int Execute(string query, params object[] args);
        List<T> Query<T>(string query, params object[] args) where T : new();
        int UpdateAll(IEnumerable objects, bool runInTransaction = true);
        int Update(object obj);
        int Insert(object obj);
        int InsertAll(IEnumerable objects, bool runInTransaction = true);
        int InsertOrReplace(object obj);
        T Get<T>(long primaryKey) where T : new();
        void Delete<T>(long primaryKey);
        TableQuery<T> Table<T>() where T : new();
        void RunInTransaction(Action action);
        void Dispose();
    }
}