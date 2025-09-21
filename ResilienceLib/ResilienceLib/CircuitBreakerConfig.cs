namespace ResilienceLib;

public class CircuitBreakerConfig   
{
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 2;
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan? HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(5);
}