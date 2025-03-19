using System.Net.Sockets;

namespace SimpleProxy.Http.Strategies;

internal sealed class ProxyHttpsNoForwardStrategy(TcpClient client) : IProxyStrategy
{
    private static readonly byte[] Message = "HTTP/1.1 200 Connection Established\r\n\r\n"u8.ToArray();

    public async Task<TcpClient> ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token)
    {
        var stream = client.GetStream();
        await stream.WriteAsync(Message, token);

        var bytes = new byte[65536];
        var count = await stream.ReadAsync(bytes, token);

        var server = new TcpClient();
        await server.ConnectAsync(host, port, token);

        await server.GetStream().WriteAsync(bytes, 0, count, token);
        return server;
    }

    public void Dispose()
    {
    }
}