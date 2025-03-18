using System.Net;
using System.Net.Sockets;

namespace SimpleProxy.Http;

public readonly struct HttpClient(TcpClient client, IPEndPoint endPoint)
{
    public TcpClient Client { get; } = client;

    public IPEndPoint EndPoint { get; } = endPoint;
}