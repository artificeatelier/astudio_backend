using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;

public class FakeReviewRepository : IReviewRepository
{
    private readonly List<Review> _reviews = new();

    public Task<Review> InsertAsync(Review review)
    {
        review.Id = review.Id ?? System.Guid.NewGuid().ToString();
        _reviews.Insert(0, review); // newest first, matching real sort order
        return Task.FromResult(review);
    }

    public Task<(List<Review> Items, bool HasMore)> GetPageAsync(int skip, int limit)
    {
        var fetched = _reviews.Skip(skip).Take(limit + 1).ToList();
        var (items, hasMore) = PaginationHelper.TrimForPage(fetched, limit);
        return Task.FromResult((items, hasMore));
    }
}
