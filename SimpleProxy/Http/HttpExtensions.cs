namespace SimpleProxy.Http;

public static class HttpExtensions
{
    public static void AddHttpConnection(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionManager, HttpConnectionManager>();
    }
}