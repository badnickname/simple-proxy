namespace SimpleProxy;

public sealed class LoggerTunnel(ITunnel tunnel, ILogger<ITunnel> logger) : ITunnel
{
    public async Task ProcessAsync(CancellationToken token)
    {
        using var activity = new System.Diagnostics.Activity("Connection");
        activity.Start();
        try
        {
            await tunnel.ProcessAsync(token);
        }
        catch (Exception e)
        {
            logger.LogError("Connection failed: {Exception}", e);
        }
        finally
        {
            tunnel.Dispose();
            activity.Stop();
            logger.LogInformation("Connection {Host}:{Port} - {Time}ms", tunnel.Host, tunnel.Port, activity.Duration.TotalMilliseconds);
        }
    }

    public string Host => tunnel.Host;

    public int Port => tunnel.Port;

    public void Dispose()
    {
        tunnel.Dispose();
    }
}