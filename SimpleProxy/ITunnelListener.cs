namespace SimpleProxy;

/// <summary>
///     Обработчик запросов на создание прокси
/// </summary>
public interface ITunnelListener
{
    IAsyncEnumerable<ITunnel> ListenAsync(string host, int port, CancellationToken token);
}