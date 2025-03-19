using System.Net.Sockets;

namespace SimpleProxy.Http;

internal sealed class HttpTunnel(TcpClient client) : ITunnel
{
    private readonly byte[] _buffer = new byte[65536];
    private readonly HttpRemoteConnectionFactory _keeper = new();
    private TcpClient? _server;
    private CancellationToken _token;

    public async Task ProcessAsync(CancellationToken token)
    {
        _token = token;

        // Construct
        var stream = client.GetStream();
        var count = await stream.ReadAsync(_buffer, 0, _buffer.Length, _token);
        var remote = _keeper.GetProxyStrategy(new Span<byte>(_buffer, 0, count), client);

        // Tunnel
        await remote.ConnectAsync(_keeper.Host!, _keeper.Port, new Memory<byte>(_buffer, 0, count), _token);
        await remote.TunnelAsync(client, ListenAsync, token);
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
}