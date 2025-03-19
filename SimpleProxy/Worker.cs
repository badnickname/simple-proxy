using Microsoft.Extensions.Options;

namespace SimpleProxy;

public sealed class Worker(IOptions<ProxyOption> option, IConnectionManager manager, ILogger<IConnection> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ThreadPool.SetMaxThreads(option.Value.Threads, option.Value.Threads);
        await foreach (var connection in manager.ListenAsync(option.Value.Host, option.Value.Port, stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested) break;
            var wrappedConnection = new LoggerConnection(connection, logger);
            _ = Task.Run(() => wrappedConnection.ProcessAsync(stoppingToken), stoppingToken);
        }
    }
}