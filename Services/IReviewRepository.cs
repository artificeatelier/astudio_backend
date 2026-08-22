using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services
{
    public interface IReviewRepository
    {
        Task<Review> InsertAsync(Review review);
        Task<(List<Review> Items, bool HasMore)> GetPageAsync(int skip, int limit);
    }
}
