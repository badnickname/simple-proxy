using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleProxy.Http.Strategies;

namespace SimpleProxy.Http;

internal sealed class HttpRemoteServerKeeper : IDisposable
{
    private const int HostHash = 504;
    private const int ConnectHash = 408;
    private (TcpClient, bool)? _cached;
    private TcpClient _client;
    private readonly byte[] _hostBytes = [72, 111, 115, 116, 58, 32];
    private readonly byte[] _connectBytes = [67, 79, 78, 78, 69, 67, 84, 32];
    private const int HttpNoForward = 0;
    private const int HttpsNoForward = 1;
    private IProxyStrategy? _strategy;

    public string? Host { get; private set; }

    public int Port { get; private set; }

    public async ValueTask<(TcpClient, bool)> GetServerAsync(Memory<byte> bytes, TcpClient client, CancellationToken token)
    {
        if (_cached.HasValue) return _cached.Value;
        _client = client;
        _strategy = CreateProxyStrategy(bytes);
        var server = await _strategy.ConnectAsync(Host!, Port, bytes, token);
        _cached = (server, false);
        return (server, true);
    }

    private IProxyStrategy CreateProxyStrategy(Memory<byte> bytes)
    {
        var content = bytes.Span;

        // Найти Host по алгоритму Рабина-Карпа
        var hash = 0;
        var l = 0;
        const int n = 6;
        for (var i = 0; i < content.Length; i++)
        {
            // Найти по хешу строку с Host
            l++;
            hash += content[i];
            if (l < n) continue;
            if (l > n)
            {
                l--;
                hash -= content[i - n];
            }

            // Проверить, что найденное по хэшу слово совпадает
            switch (hash)
            {
                case HostHash:
                {
                    if (!CheckWord(content[(i - n + 1)..], _hostBytes, 6)) break;
                    var split = GetHostName(content[(i + 1)..]);
                    ExtractHostAndPort(split);
                    return CreateProxyStrategy(HttpNoForward);
                }
                case ConnectHash:
                {
                    if (!CheckWord(content[(i - n - 1)..], _connectBytes, 8)) break;
                    var split = GetHostName(content[(i + 1)..]);
                    ExtractHostAndPort(split);
                    return CreateProxyStrategy(HttpsNoForward);
                }
                default:
                    continue;
            }
        }

        throw new ProtocolViolationException("The request is missing a Host header");
    }

    private static bool CheckWord(Span<byte> content, byte[] word, int n)
    {
        var success = true;
        for (var j = 0; j < n; j++)
            if (content[j] != word[j])
            {
                success = false;
                break;
            }

        return success;
    }

    private void ExtractHostAndPort(List<string> split)
    {
        Port = split.Count == 2 ? int.Parse(split[1]) : 80;
        Host = split[0];
    }
    
    private static List<string> GetHostName(Span<byte> content)
    {
        var sb = new StringBuilder();
        var list = new List<string>();
        foreach (var t in content)
        {
            var ch = (char) t;
            switch (ch)
            {
                case ':':
                    list.Add(sb.ToString());
                    sb.Clear();
                    continue;
                case ' ' or '\n' or '\r':
                    list.Add(sb.ToString());
                    return list;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        return list;
    }

    private IProxyStrategy CreateProxyStrategy(int strategy)
    {
        return strategy switch
        {
            HttpNoForward => new ProxyHttpNoForwardStrategy(),
            HttpsNoForward => new ProxyHttpsNoForwardStrategy(_client),
            _ => throw new ProtocolViolationException("Incompatible HTTP strategy type")
        };
    }

    public void Dispose()
    {
        _cached?.Item1.Dispose();
        _strategy?.Dispose();
    }
}