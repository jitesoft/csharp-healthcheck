using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jitesoft.Lib.Health;

/// <summary>
/// Extension class to initialize healthcheck service with.
/// </summary>
public static class Extension
{
    public static IHealthChecksBuilder UseHealthChecks(this IServiceCollection self, Action<HealthCheckOptions> configurationBuilder)
    {
        self.ConfigureHealthCheck(configurationBuilder);
        return self.UseHealthChecks();
    }

    public static IHealthChecksBuilder UseHealthChecks(this IServiceCollection self)
    {
        return self
            .AddHostedService<HealthService>()
            .AddHealthChecks();
    }

    public static IServiceCollection ConfigureHealthCheck(this IServiceCollection self, IConfiguration configuration)
    {
        return self
            .AddOptions()
            .Configure<HealthCheckOptions>(configuration);
    }

    public static IServiceCollection ConfigureHealthCheck(this IServiceCollection self, Action<HealthCheckOptions> configurationBuilder)
    {
        return self
            .AddOptions()
            .Configure<HealthCheckOptions>(configurationBuilder);
    }
}
