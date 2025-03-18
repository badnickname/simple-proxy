using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SimpleProxy.Http;

internal sealed class HttpConnection(TcpClient client) : IConnection, IDisposable
{
    private readonly byte[] _buffer = new byte[65536];
    private readonly Dictionary<TcpClient, TcpConnectionInformation> _connections = new();
    private readonly Queue<Func<Task>> _queue = new();
    private readonly HttpRemoteServerKeeper _keeper = new();
    private readonly byte[] _serverBuffer = new byte[65536];
    private TcpClient? _server;
    private CancellationToken _token;

    public async Task ProcessAsync(CancellationToken token)
    {
        _token = token;
        _queue.Enqueue(ReceiveFromClientAsync);
        try
        {
            while (!_token.IsCancellationRequested && _queue.TryDequeue(out var action)) await action();
        }
        finally
        {
            Dispose();
        }
    }

    public string Host => _keeper.Host!;

    public int Port => _keeper.Port;

    public void Dispose()
    {
        client.Dispose();
        _server?.Dispose();
    }

    private async Task ReceiveFromClientAsync()
    {
        if (!IsActive(client)) return;
        var stream = client.GetStream();
        var count = await stream.ReadAsync(_buffer, 0, _buffer.Length, _token);
        if (count > 0) _queue.Enqueue(async () => await SendToServerAsync(new Memory<byte>(_buffer, 0, count)));
    }

    private async Task SendToClientAsync(Memory<byte> content)
    {
        var stream = client.GetStream();
        await stream.WriteAsync(content, _token);
        _queue.Enqueue(ReceiveFromClientAsync);
    }

    private async Task SendToServerAsync(Memory<byte> content)
    {
        var (server, first) = await _keeper.GetServerAsync(content, client, _token);
        _server = server;
        if (!first) await _server.GetStream().WriteAsync(content, _token);
        _queue.Enqueue(ReceiveFromServerAsync);
    }

    private async Task ReceiveFromServerAsync()
    {
        if (!IsActive(_server!)) return;
        var stream = _server!.GetStream();
        var count = await stream.ReadAsync(_serverBuffer, 0, _serverBuffer.Length, _token);
        if (count > 0) _queue.Enqueue(async () => await SendToClientAsync(new Memory<byte>(_serverBuffer, 0, count)));
    }

    private bool IsActive(TcpClient tcpClient)
    {
        TcpState state;
        if (_connections.TryGetValue(tcpClient, out var connection))
        {
            state = connection.State;
        }
        else
        {
            var connections = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpConnections();
            connection = Array.Find(connections, x => x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint));
            if (connection is not null) _connections.Add(tcpClient, connection);
            state = connection?.State ?? TcpState.Unknown;
        }

        return state is TcpState.Established or TcpState.Listen or TcpState.SynReceived or TcpState.SynSent or TcpState.TimeWait or TcpState.Unknown;
    }
}