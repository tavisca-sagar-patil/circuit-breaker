namespace ResilienceLib;

public interface IConnector
{
    Task<HttpResponseMessage> CallApiAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
}