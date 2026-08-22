using System;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _repository;
        private readonly InMemoryRateLimiter _rateLimiter;

        public ReviewsController(IReviewRepository repository, InMemoryRateLimiter rateLimiter)
        {
            _repository = repository;
            _rateLimiter = rateLimiter;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? skip, [FromQuery] int? limit)
        {
            var clampedSkip = PaginationHelper.ClampSkip(skip);
            var clampedLimit = PaginationHelper.ClampLimit(limit);
            var (items, hasMore) = await _repository.GetPageAsync(clampedSkip, clampedLimit);
            return Ok(new { items, hasMore });
        }

        [HttpPost]
        public async Task<IActionResult> Post(ReviewCreateRequest request)
        {
            var errors = ReviewValidator.Validate(request);
            if (errors.Count > 0)
                return BadRequest(new { errors });

            var ip = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            if (!_rateLimiter.TryRegister(ip, DateTime.UtcNow, TimeSpan.FromSeconds(60)))
                return StatusCode(429, new { message = "Please wait a minute before submitting another review." });

            var review = new Review
            {
                Name = request.Name.Trim(),
                Rating = request.Rating,
                Text = request.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var saved = await _repository.InsertAsync(review);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving review: {ex.Message}");
                return StatusCode(500, new { message = "Could not save review, please try again." });
            }
        }
    }
}
