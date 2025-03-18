using System.Net.Sockets;

namespace SimpleProxy.Http.Strategies;

internal interface IProxyStrategy : IDisposable
{
    Task<TcpClient> ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token);
}
