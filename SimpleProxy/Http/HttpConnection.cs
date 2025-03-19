using System.Net.Sockets;

namespace SimpleProxy.Http;

internal sealed class HttpConnection(TcpClient client) : IConnection
{
    private readonly byte[] _buffer = new byte[65536];
    private readonly HttpRemoteConnection _keeper = new();
    private readonly byte[] _serverBuffer = new byte[65536];
    private TcpClient? _server;
    private CancellationToken _token;

    public async Task ProcessAsync(CancellationToken token)
    {
        _token = token;

        // Connect
        var stream = client.GetStream();
        var count = await stream.ReadAsync(_buffer, 0, _buffer.Length, _token);
        _server = await _keeper.GetServerAsync(new Memory<byte>(_buffer, 0, count), client, _token);

        // Tunnel
        switch (_keeper.Flow)
        {
            case HttpRemoteConnection.HttpNoForward:
                await ListenAsync(_server, client, _serverBuffer);
                return;
            case HttpRemoteConnection.HttpsNoForward:
                await Task.WhenAll(TunnelAsync(_server, client, _serverBuffer), TunnelAsync(client, _server, _buffer));
                return;
        }
    }

    public string Host => _keeper.Host!;

    public int Port => _keeper.Port;

    public void Dispose()
    {
        client.GetStream().Dispose();
        client.Dispose();
        _server?.GetStream().Dispose();
        _server?.Dispose();
    }

    private async Task<bool> ListenAsync(TcpClient from, TcpClient to, byte[] buffer)
    {
        var count = await from.GetStream().ReadAsync(buffer, _token);
        if (count < 1) return false;

        if (!to.Connected) return false;
        await to.GetStream().WriteAsync(new Memory<byte>(buffer, 0, count), _token);
        return true;
    }
    
    private async Task TunnelAsync(TcpClient from, TcpClient to, byte[] buffer)
    {
        while (!_token.IsCancellationRequested && from.Connected && to.Connected)
        {
            if (!await ListenAsync(from, to, buffer)) return;
        }
    }
}