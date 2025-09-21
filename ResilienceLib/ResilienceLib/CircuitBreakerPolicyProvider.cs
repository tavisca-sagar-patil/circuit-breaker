namespace ResilienceLib
{
    using Polly;

    public static class CircuitBreakerPolicyProvider
    {
        private static IAsyncPolicy<HttpResponseMessage> GetResiliencyPolicy(CircuitBreakerConfig circuitBreakerConfig)
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(resp => !resp.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    circuitBreakerConfig.RetryCount,
                    attempt => circuitBreakerConfig.RetryDelay);

            IAsyncPolicy<HttpResponseMessage> circuitPolicy;
            if (circuitBreakerConfig.HealthCheckInterval.HasValue)
            {
                circuitPolicy = Policy.Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(resp => !resp.IsSuccessStatusCode)
                    .AdvancedCircuitBreakerAsync(
                        failureThreshold: 0.5,
                        samplingDuration: circuitBreakerConfig.HealthCheckInterval.Value,
                        minimumThroughput: 5,
                        durationOfBreak: circuitBreakerConfig.BreakDuration);
            }
            else
            {
                circuitPolicy = Policy.Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(resp => !resp.IsSuccessStatusCode)
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: circuitBreakerConfig.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: circuitBreakerConfig.BreakDuration);
            }

            return Policy.WrapAsync(retryPolicy, circuitPolicy);
        }
    }
}