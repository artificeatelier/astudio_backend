using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using MongoDB.Driver;

namespace backend.Services
{
    public class MongoReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<Review> _collection;

        public MongoReviewRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Review>("reviews");
        }

        public async Task<Review> InsertAsync(Review review)
        {
            await _collection.InsertOneAsync(review);
            return review;
        }

        public async Task<(List<Review> Items, bool HasMore)> GetPageAsync(int skip, int limit)
        {
            var fetched = await _collection
                .Find(FilterDefinition<Review>.Empty)
                .SortByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Limit(limit + 1)
                .ToListAsync();

            return PaginationHelper.TrimForPage(fetched, limit);
        }
    }
}
