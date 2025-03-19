using System.Net.Sockets;

namespace SimpleProxy.Http.RemoteConnection;

internal sealed class ProxyHttpsNoForwardStrategy(TcpClient tcpClient) : IRemoteConnection
{
    private static readonly byte[] Message = "HTTP/1.1 200 Connection Established\r\n\r\n"u8.ToArray();
    private readonly byte[] _buffer = new byte[65536];
    private readonly byte[] _clientBuffer = new byte[65536];
    private TcpClient? _server;

    public async Task ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token)
    {
        var stream = tcpClient.GetStream();
        await stream.WriteAsync(Message, token);

        var bytes = new byte[65536];
        var count = await stream.ReadAsync(bytes, token);

        _server = new TcpClient();
        await _server.ConnectAsync(host, port, token);

        await _server.GetStream().WriteAsync(bytes, 0, count, token);
    }

    public async Task TunnelAsync(TcpClient client, Func<TcpClient, TcpClient, byte[], Task<bool>> listen, CancellationToken token)
        => await Task.WhenAll(TunnelAsync(_server!, client, _buffer, listen, token), TunnelAsync(client, _server!, _clientBuffer, listen, token));

    private static async Task TunnelAsync(TcpClient from, TcpClient to, byte[] buffer, Func<TcpClient, TcpClient, byte[], Task<bool>> listen, CancellationToken token)
    {
        while (from.Connected && to.Connected && !token.IsCancellationRequested)
        {
            if (!await listen(from, to, buffer)) return;
        }
    }

    public void Dispose()
    {
        _server?.Dispose();
    }
}