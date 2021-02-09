using System;
using FluentMigrator.Runner;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = XRayBuilder.Core.Libraries.Logging.ILogger;
using LogLevel = XRayBuilder.Core.Libraries.Logging.LogLevel;

namespace XRayBuilder.Core.Database
{
    [UsedImplicitly]
    public class DatabaseMigrator
    {
        private readonly DatabaseConfig _config;
        private readonly ILogger _logger;

        public DatabaseMigrator(DatabaseConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Migrate()
        {
            var serviceProvider = CreateServices();

            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            _logger.Log("Starting database migrations...");
            runner.MigrateUp();
            _logger.Log("All up to date!");
        }

        private IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString($"Data Source={_config.Filename};Version=3;DateTimeKind=Utc")
                    .ScanIn(typeof(DatabaseMigrator).Assembly).For.Migrations())
#if DEBUG
                .AddLogging(lb => lb.AddProvider(new LoggerProvider(_logger)))
#endif
                .BuildServiceProvider();
        }

        private sealed class LoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public LoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new Logger(_logger);

            public void Dispose() { }
        }

        private sealed class Logger : Microsoft.Extensions.Logging.ILogger
        {
            private readonly ILogger _logger;

            public Logger(ILogger logger)
            {
                _logger = logger;
            }

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _logger.Log(formatter(state, exception), logLevel switch
                {
                    Microsoft.Extensions.Logging.LogLevel.None => LogLevel.Auto,
                    Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Auto,
                    Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Info,
                    Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
                    Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warn,
                    Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
                    Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Error,
                    _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
                });
            }

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => new FakeDisposable();

            private sealed class FakeDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }
    }
}
