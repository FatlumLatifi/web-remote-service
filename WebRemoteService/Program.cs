using Microsoft.Extensions.Hosting;
using ServiceRelease;
var builder = Host.CreateApplicationBuilder();

builder.Services.AddSystemd();
builder.Services.AddHostedService<Worker>();
var host = builder.Build();

host.Run();