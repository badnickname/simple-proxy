namespace SimpleProxy;

public interface IConnectionManager
{
    IAsyncEnumerable<IConnection> ListenAsync(string host, int port, CancellationToken token);
}