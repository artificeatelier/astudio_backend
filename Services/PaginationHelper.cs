using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public static class PaginationHelper
    {
        public const int DefaultLimit = 6;
        public const int MaxLimit = 24;

        public static int ClampLimit(int? requested)
        {
            if (requested == null || requested <= 0) return DefaultLimit;
            return Math.Min(requested.Value, MaxLimit);
        }

        public static int ClampSkip(int? requested)
        {
            if (requested == null || requested < 0) return 0;
            return requested.Value;
        }

        // Caller fetches up to `limit + 1` rows; this trims the extra
        // probe row and reports whether one existed (i.e. hasMore).
        public static (List<T> Items, bool HasMore) TrimForPage<T>(List<T> fetched, int limit)
        {
            bool hasMore = fetched.Count > limit;
            var items = hasMore ? fetched.Take(limit).ToList() : fetched;
            return (items, hasMore);
        }
    }
}
