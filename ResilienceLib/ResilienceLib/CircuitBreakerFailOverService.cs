using Polly;
using Polly.CircuitBreaker;
using System.Net.Http;

namespace ResilienceLib;

public class CircuitBreakerFailOverService
{
    private readonly CircuitBreakerConfig _config;
    private readonly IList<IConnector> _connectors;
    private readonly IList<Func<CancellationToken, Task<bool>>> _healthChecks;
    private readonly IList<AsyncCircuitBreakerPolicy<HttpResponseMessage>> _policies;

    public CircuitBreakerFailOverService(
        CircuitBreakerConfig config,
        IEnumerable<IConnector> connectors,
        IEnumerable<Func<CancellationToken, Task<bool>>> healthChecks)
    {
        _config = config;
        _connectors = connectors.ToList();
        _healthChecks = healthChecks.ToList();
        _policies = _connectors.Select(_ => Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                _config.ExceptionsAllowedBeforeBreaking,
                _config.BreakDuration)
        ).ToList();
    }

    public async Task<HttpResponseMessage> CallAsync(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < _connectors.Count; i++)
        {
            var policy = _policies[i];
            if (policy.CircuitState == CircuitState.Open)
            {
                if (policy.LastException != null
                   // policy.LastHandledTime.HasValue &&
                   // (DateTime.UtcNow - policy.LastHandledTime.Value) > _config.BreakDuration
                   ){
                    if (await _healthChecks[i](cancellationToken))
                    {
                        policy.Reset();
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }

            try
            {
                return await policy.ExecuteAsync(ct => _connectors[i].CallApiAsync(ct), cancellationToken);
            }
            catch (BrokenCircuitException)
            {
                continue;
            }
        }
        throw new Exception("All connectors failed.");
    }
}
