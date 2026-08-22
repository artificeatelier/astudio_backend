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
        private readonly ITranslationService _translationService;

        public ReviewsController(IReviewRepository repository, InMemoryRateLimiter rateLimiter, ITranslationService translationService)
        {
            _repository = repository;
            _rateLimiter = rateLimiter;
            _translationService = translationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? skip, [FromQuery] int? limit)
        {
            var clampedSkip = PaginationHelper.ClampSkip(skip);
            var clampedLimit = PaginationHelper.ClampLimit(limit);

            try
            {
                var (items, hasMore) = await _repository.GetPageAsync(clampedSkip, clampedLimit);
                return Ok(new { items, hasMore });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching reviews: {ex.Message}");
                return StatusCode(500, new { message = "Could not load reviews, please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(ReviewCreateRequest request)
        {
            var errors = ReviewValidator.Validate(request);
            if (errors.Count > 0)
                return BadRequest(new { errors });

            var ip = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            if (!_rateLimiter.TryRegister(ip, DateTime.UtcNow, TimeSpan.FromSeconds(10)))
                return StatusCode(429, new { message = "Please wait a few seconds before submitting another review." });

            var trimmedText = request.Text!.Trim();

            // Translation is a nice-to-have on top of the review, not a
            // requirement for it — any failure here (unconfigured key,
            // DeepL outage, network error) must not block saving the review.
            string? translatedText = null;
            string? sourceLang = null;
            try
            {
                var translation = await _translationService.TranslateAsync(trimmedText);
                if (translation != null)
                {
                    translatedText = translation.TranslatedText;
                    sourceLang = translation.SourceLang;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Translation failed, saving review without it: {ex.Message}");
            }

            var review = new Review
            {
                Name = request.Name!.Trim(),
                Rating = request.Rating,
                Text = trimmedText,
                TranslatedText = translatedText,
                SourceLang = sourceLang,
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
