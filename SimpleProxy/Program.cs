using SimpleProxy;
using SimpleProxy.Http;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpConnection();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();