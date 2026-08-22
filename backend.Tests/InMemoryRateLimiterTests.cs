using System;
using backend.Services;
using Xunit;

public class InMemoryRateLimiterTests
{
    [Fact]
    public void First_request_from_an_ip_is_allowed()
    {
        var limiter = new InMemoryRateLimiter();
        var allowed = limiter.TryRegister("1.2.3.4", DateTime.UtcNow, TimeSpan.FromSeconds(60));
        Assert.True(allowed);
    }

    [Fact]
    public void Second_request_within_window_is_blocked()
    {
        var limiter = new InMemoryRateLimiter();
        var t0 = DateTime.UtcNow;
        limiter.TryRegister("1.2.3.4", t0, TimeSpan.FromSeconds(60));
        var allowed = limiter.TryRegister("1.2.3.4", t0.AddSeconds(30), TimeSpan.FromSeconds(60));
        Assert.False(allowed);
    }

    [Fact]
    public void Request_after_window_elapses_is_allowed()
    {
        var limiter = new InMemoryRateLimiter();
        var t0 = DateTime.UtcNow;
        limiter.TryRegister("1.2.3.4", t0, TimeSpan.FromSeconds(60));
        var allowed = limiter.TryRegister("1.2.3.4", t0.AddSeconds(61), TimeSpan.FromSeconds(60));
        Assert.True(allowed);
    }

    [Fact]
    public void Different_ips_are_tracked_independently()
    {
        var limiter = new InMemoryRateLimiter();
        var t0 = DateTime.UtcNow;
        limiter.TryRegister("1.2.3.4", t0, TimeSpan.FromSeconds(60));
        var allowed = limiter.TryRegister("5.6.7.8", t0, TimeSpan.FromSeconds(60));
        Assert.True(allowed);
    }
}
