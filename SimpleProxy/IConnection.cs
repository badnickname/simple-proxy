namespace SimpleProxy;

public interface IConnection
{
    Task ProcessAsync(CancellationToken token);

    string Host { get; }

    int Port { get; }
}