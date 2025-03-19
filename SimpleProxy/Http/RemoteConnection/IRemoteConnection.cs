using System.Net.Sockets;

namespace SimpleProxy.Http.RemoteConnection;

/// <summary>
///     Соединение с удаленным сервером
/// </summary>
internal interface IRemoteConnection : IDisposable
{
    /// <summary>
    ///     Установить соединение
    /// </summary>
    /// <param name="host">Хост</param>
    /// <param name="port">Порт</param>
    /// <param name="buffer">Буффер с запросом от клиента</param>
    /// <param name="token">Токен отмены</param>
    Task ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token);

    /// <summary>
    ///     Получить ответ от удаленного сервера
    /// </summary>
    /// <param name="client">Клиент</param>
    /// <param name="listen">Функция для прослушивания (TcpClient от_кого, TcpClient кому, byte[] буффер) => были ли полученны байты</param>
    /// <param name="token">Токен отмены</param>
    Task TunnelAsync(TcpClient client, Func<TcpClient, TcpClient, byte[], Task<bool>> listen, CancellationToken token);
}