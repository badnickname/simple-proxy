namespace SimpleProxy;

public interface IConnection : IDisposable
{
    Task ProcessAsync(CancellationToken token);

    string Host { get; }

    int Port { get; }
}