using System.Net.Sockets;

namespace SimpleProxy.Http.RemoteConnection;

internal sealed class ProxyHttpNoForwardRemoteConnection : IRemoteConnection
{
    private readonly byte[] _buffer = new byte[65536];
    private TcpClient? _server;

    public async Task ConnectAsync(string host, int port, Memory<byte> buffer, CancellationToken token)
    {
        _server = new TcpClient();
        await _server.ConnectAsync(host, port, token);
        var stream = _server.GetStream();
        await stream.WriteAsync(buffer, token);
    }

    public Task TunnelAsync(TcpClient client, Func<TcpClient, TcpClient, byte[], Task<bool>> listen, CancellationToken token) => listen(_server!, client, _buffer);

    public void Dispose()
    {
        _server?.Dispose();
    }
}