namespace Jitesoft.Lib.Health;

/// <summary>
/// Health Check Options
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Port to use for the health check service.
    /// </summary>
    public short Port { get; set; } = 5000;

    /// <summary>
    /// Endpoint to access the healthcheck service on.
    /// </summary>
    public string Route { get; set; } = "/health";
}
