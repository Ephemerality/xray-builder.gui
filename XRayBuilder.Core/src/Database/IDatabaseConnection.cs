using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilder.Core.Database
{
    /** TODO LIST **/
    /*
     * error handling on db init
     * decide where responsiblity lies for updating database
        * db needs to be bootstrapped in Core instead of UI if it will be updated in misc services
            * this would mean Console would need to init the db even if unused
     * make writing repos less tedious and repetitive
     * find best place to assign data source ids from URL (static parse X Id inside the repo w/ regex isn't good)
     */
    public interface IDatabaseConnection : IDisposable
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteAsync(string sql, object param = null, CancellationToken cancellationToken = default);
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default);
        Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}