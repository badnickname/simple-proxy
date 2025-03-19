namespace SimpleProxy;

public sealed class LoggerConnection(IConnection connection, ILogger<IConnection> logger) : IConnection
{
    public async Task ProcessAsync(CancellationToken token)
    {
        using var activity = new System.Diagnostics.Activity("Connection");
        activity.Start();
        try
        {
            await connection.ProcessAsync(token);
        }
        catch (Exception e)
        {
            logger.LogError("Connection failed: {Exception}", e);
        }
        finally
        {
            connection.Dispose();
            activity.Stop();
            logger.LogInformation("Connection {Host}:{Port} - {Time}ms", connection.Host, connection.Port, activity.Duration.TotalMilliseconds);
        }
    }

    public string Host => connection.Host;

    public int Port => connection.Port;

    public void Dispose()
    {
        connection.Dispose();
    }
}