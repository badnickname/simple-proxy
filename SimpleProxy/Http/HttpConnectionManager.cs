using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace SimpleProxy.Http;

internal sealed class HttpConnectionManager(ILogger<IConnection> logger) : IConnectionManager, IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Any, 3131);

    public async IAsyncEnumerable<IConnection> ListenAsync([EnumeratorCancellation] CancellationToken token)
    {
        _listener.Start();
        while (!token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(token);
            client.NoDelay = true;
            client.Client.NoDelay = true;
            yield return new LoggerConnection(new HttpConnection(client), logger);
        }
    }

    public void Dispose()
    {
        _listener.Dispose();
    }
}