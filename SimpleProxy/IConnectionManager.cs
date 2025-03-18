namespace SimpleProxy;

public interface IConnectionManager
{
    IAsyncEnumerable<IConnection> ListenAsync(CancellationToken token);
}