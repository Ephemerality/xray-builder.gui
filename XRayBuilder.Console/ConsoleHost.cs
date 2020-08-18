using System;
using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilder.Console
{
    /// <summary>
    /// https://gist.github.com/YahuiWong/a1bcb1e53e600c2c9f3942e0407c9a88
    /// </summary>
    public static class ConsoleHost
    {
        /// <summary>
        /// Block the calling thread until shutdown is triggered via Ctrl+C or SIGTERM.
        /// </summary>
        public static void WaitForShutdown()
        {
            WaitForShutdownAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs an application and block the calling thread until host shutdown.
        /// </summary>
        public static void Wait()
        {
            WaitAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs an application and returns a Task that only completes when the token is triggered or shutdown is triggered.
        /// </summary>
        /// <param name="token">The token to trigger shutdown.</param>
        public static async Task WaitAsync(CancellationToken token = default)
        {
            //Wait for the token shutdown if it can be cancelled
            if (token.CanBeCanceled)
            {
                await WaitAsync(token, shutdownMessage: null);
                return;
            }

            //If token cannot be cancelled, attach Ctrl+C and SIGTERN shutdown
            var done = new ManualResetEventSlim(false);
            using var cts = new CancellationTokenSource();
            AttachCtrlcSigtermShutdown(cts, done, shutdownMessage: "Application is shutting down...");
            await WaitAsync(cts.Token, "Application running. Press Ctrl+C to shut down.");
            done.Set();
        }

        /// <summary>
        /// Returns a Task that completes when shutdown is triggered via the given token, Ctrl+C or SIGTERM.
        /// </summary>
        /// <param name="token">The token to trigger shutdown.</param>
        public static async Task WaitForShutdownAsync(CancellationToken token = default)
        {
            var done = new ManualResetEventSlim(false);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            AttachCtrlcSigtermShutdown(cts, done, shutdownMessage: string.Empty);
            await WaitForTokenShutdownAsync(cts.Token);
            done.Set();
        }

        public static Task<int> WaitForShutdownAsync(Func<CancellationToken, Task<int>> process)
        {
            var done = new ManualResetEventSlim(false);
            using var cts = new CancellationTokenSource();
            AttachCtrlcSigtermShutdown(cts, done, string.Empty);

            try
            {
                return process(cts.Token);
            }
            finally
            {
                done.Set();
            }
        }

        private static async Task WaitAsync(CancellationToken token, string shutdownMessage)
        {
            if (!string.IsNullOrEmpty(shutdownMessage))
                System.Console.WriteLine(shutdownMessage);

            await WaitForTokenShutdownAsync(token);
        }

        private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent, string shutdownMessage)
        {
            void ShutDown()
            {
                if (!cts.IsCancellationRequested)
                {
                    if (!string.IsNullOrWhiteSpace(shutdownMessage))
                        System.Console.WriteLine(shutdownMessage);

                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Ignored
                    }
                }

                //Wait on the given reset event
                resetEvent.Wait();
            }

            AppDomain.CurrentDomain.ProcessExit += delegate { ShutDown(); };
            System.Console.CancelKeyPress += (sender, eventArgs) =>
            {
                ShutDown();
                //Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private static async Task WaitForTokenShutdownAsync(CancellationToken token)
        {
            var waitForStop = new TaskCompletionSource<object>();
            token.Register(obj =>
            {
                var tcs = (TaskCompletionSource<object>) obj;
                tcs.TrySetResult(null);
            }, waitForStop);
            await waitForStop.Task;
        }
    }
}