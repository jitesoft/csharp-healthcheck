using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jitesoft.Lib.Health;

public class HealthService : IHostedService
{
     private readonly ILogger<HealthService> _logger;
    private readonly HealthCheckService _healthCheckService;
    private readonly HealthCheckOptions _healthCheckOptions;
    private WebApplication? _server = null;

    public HealthService(ILogger<HealthService> logger, IOptions<HealthCheckOptions> options, HealthCheckService healthCheckService)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
        _healthCheckOptions = options.Value;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Setting up kestrel server for health checks on port {Port}",
            _healthCheckOptions.Port
        );

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(o =>
            {
                o.Listen(IPAddress.Any, (int)_healthCheckOptions.Port);
            })
            .ConfigureLogging(x => x.ClearProviders())
            .UseIISIntegration();
        _server = builder.Build();

        _logger.LogDebug(
            "Setting up endpoint listener for health checks on {Endpoint}",
            _healthCheckOptions.Route
        );
        _server.MapGet(_healthCheckOptions.Route, async (context) =>
        {
            var health = await _healthCheckService.CheckHealthAsync(cancellationToken);
            context.Response.StatusCode = health.Status == HealthStatus.Unhealthy ? 500 : 200;
            await context.Response.WriteAsJsonAsync(health, cancellationToken);
        });

        _logger.LogInformation("Starting healthcheck service on 0.0.0.0:{Port}{Endpoint}", _healthCheckOptions.Port, _healthCheckOptions.Route);
        await _server.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping health check service");
        if (_server != null)
        {
            await _server.StopAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("Web server not running");
        }
    }
}
