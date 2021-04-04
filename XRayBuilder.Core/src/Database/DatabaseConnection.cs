using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace XRayBuilder.Core.Database
{
    public sealed record DatabaseConfig(string Filename);

    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly SQLiteConnection _sqLiteConnection;

        private bool _opened;

        public DatabaseConnection(DatabaseConfig config)
        {
            _sqLiteConnection = new SQLiteConnection(config.Filename);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);
            return await _sqLiteConnection.QueryAsync<T>(new CommandDefinition(sql, param, null, cancellationToken: cancellationToken));
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);
            return await _sqLiteConnection.ExecuteAsync(new CommandDefinition(sql, param, null, cancellationToken: cancellationToken));
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, CancellationToken cancellationToken = default)
        {
            await OpenAsync(cancellationToken);
            return await _sqLiteConnection.ExecuteScalarAsync<T>(new CommandDefinition(sql, param, null, cancellationToken: cancellationToken));
        }

        public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            await OpenAsync(cancellationToken);
            return _sqLiteConnection.BeginTransaction(IsolationLevel.Serializable);
        }

        private Task OpenAsync(CancellationToken cancellationToken)
        {
            if (!_opened)
            {
                _opened = true;
                return _sqLiteConnection.OpenAsync(cancellationToken);
            }

            // Database became closed after we opened it somehow
            // If in a transaction, we'll need to abort since we can't guarantee the state anymore
            // If not, just try to re-open it
            // if (_sqLiteConnection.State == ConnectionState.Closed)
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _sqLiteConnection?.Dispose();
        }
    }
}