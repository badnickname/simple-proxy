using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace SimpleProxy.Http;

internal sealed class HttpTunnelListener : ITunnelListener, IDisposable
{
    private TcpListener _listener;

    public async IAsyncEnumerable<ITunnel> ListenAsync(string host, int port, [EnumeratorCancellation] CancellationToken token)
    {
        _listener = new TcpListener(IPAddress.Parse(host), port);
        _listener.Start();
        while (!token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(token);
            yield return new HttpTunnel(client);
        }
    }

    public void Dispose()
    {
        _listener.Dispose();
    }
}