using System;
using System.Collections.Concurrent;

namespace backend.Services
{
    public class InMemoryRateLimiter
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastPostAtByIp = new();

        public bool TryRegister(string ip, DateTime now, TimeSpan window)
        {
            if (_lastPostAtByIp.TryGetValue(ip, out var last) && now - last < window)
            {
                return false;
            }
            _lastPostAtByIp[ip] = now;
            return true;
        }
    }
}
