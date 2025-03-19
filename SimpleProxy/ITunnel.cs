namespace SimpleProxy;

/// <summary>
///     Прокси-соединение
/// </summary>
public interface ITunnel : IDisposable
{
    Task ProcessAsync(CancellationToken token);

    string Host { get; }

    int Port { get; }
}