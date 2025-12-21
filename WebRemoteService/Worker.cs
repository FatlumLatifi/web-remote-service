using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using WebRemote;

namespace ServiceRelease;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly WebApplication _webApp;

    public Worker(ILogger<Worker> logger)
    {
          _logger = logger;

        var webBuilder = WebApplication.CreateSlimBuilder();
        webBuilder.Logging.ClearProviders();
        webBuilder.Logging.AddProvider(new OwnLogger(logger));
        _webApp = WebRemoteApplication.CreateWebApplication(null, null);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       await _webApp.RunAsync(stoppingToken);
    }
}


public class OwnLogger : ILoggerProvider
{
    public OwnLogger(ILogger logger)
    {
        _logger = logger;
    }

    private ILogger _logger;
    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
    }
}
