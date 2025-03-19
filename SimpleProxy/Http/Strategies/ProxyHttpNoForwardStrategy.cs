using System.Net.Sockets;

namespace SimpleProxy.Http.Strategies;

internal sealed class ProxyHttpNoForwardStrategy : IProxyStrategy
{
    public async Task<TcpClient> ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token)
    {
        var server = new TcpClient();
        await server.ConnectAsync(host, port, token);
        var stream = server.GetStream();
        await stream.WriteAsync(buffer, token);
        return server;
    }

    public void Dispose()
    {
    }
}