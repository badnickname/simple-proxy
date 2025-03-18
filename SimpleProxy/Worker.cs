namespace SimpleProxy;

public sealed class Worker(IConnectionManager manager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ThreadPool.SetMaxThreads(2, 2);
        await foreach (var connection in manager.ListenAsync(stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested) break;
            ThreadPool.SetMaxThreads(2, 2);
            _ = Task.Run(async () => await connection.ProcessAsync(stoppingToken), stoppingToken);
        }
    }
}