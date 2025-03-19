using SimpleProxy;
using SimpleProxy.Http;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpConnection();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<ProxyOption>(builder.Configuration.GetSection("Proxy"));

var host = builder.Build();
host.Run();