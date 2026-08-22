using System;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class ReviewsControllerTests
{
    private static ReviewsController MakeController(IReviewRepository? repo = null, InMemoryRateLimiter? limiter = null)
    {
        return new ReviewsController(repo ?? new FakeReviewRepository(), limiter ?? new InMemoryRateLimiter());
    }

    [Fact]
    public async Task Get_returns_empty_page_when_no_reviews()
    {
        var controller = MakeController();
        var result = await controller.Get(null, null);
        var ok = Assert.IsType<OkObjectResult>(result);
        dynamic body = ok.Value!;
        Assert.Empty((System.Collections.Generic.IEnumerable<Review>)body.items);
        Assert.False((bool)body.hasMore);
    }

    [Fact]
    public async Task Post_with_valid_body_returns_200_and_saved_review()
    {
        var controller = MakeController();
        var request = new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = "Great work." };
        var result = await controller.Post(request);
        var ok = Assert.IsType<OkObjectResult>(result);
        var saved = Assert.IsType<Review>(ok.Value);
        Assert.Equal("Priya", saved.Name);
        Assert.Equal(5, saved.Rating);
    }

    [Fact]
    public async Task Post_with_invalid_rating_returns_400()
    {
        var controller = MakeController();
        var request = new ReviewCreateRequest { Name = "Priya", Rating = 9, Text = "Great work." };
        var result = await controller.Post(request);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_twice_quickly_from_same_ip_is_rate_limited()
    {
        var repo = new FakeReviewRepository();
        var limiter = new InMemoryRateLimiter();
        var controller = MakeController(repo, limiter);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("1.2.3.4");

        var request = new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = "Great work." };
        await controller.Post(request);
        var second = await controller.Post(request);

        var status = Assert.IsType<ObjectResult>(second);
        Assert.Equal(429, status.StatusCode);
    }

    [Fact]
    public async Task Get_after_insert_returns_it_with_hasMore_false()
    {
        var repo = new FakeReviewRepository();
        var controller = MakeController(repo);
        await controller.Post(new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = "Great work." });

        var result = await controller.Get(0, 6);
        var ok = Assert.IsType<OkObjectResult>(result);
        dynamic body = ok.Value!;
        Assert.Single((System.Collections.Generic.IEnumerable<Review>)body.items);
        Assert.False((bool)body.hasMore);
    }
}
