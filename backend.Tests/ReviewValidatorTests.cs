using System.Collections.Generic;
using backend.Models;
using backend.Services;
using Xunit;

public class ReviewValidatorTests
{
    [Fact]
    public void Valid_request_has_no_errors()
    {
        var request = new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = "Great work." };
        var errors = ReviewValidator.Validate(request);
        Assert.Empty(errors);
    }

    [Fact]
    public void Empty_name_is_rejected()
    {
        var request = new ReviewCreateRequest { Name = "", Rating = 5, Text = "Great work." };
        var errors = ReviewValidator.Validate(request);
        Assert.Contains(errors, e => e.Contains("Name"));
    }

    [Fact]
    public void Name_over_60_chars_is_rejected()
    {
        var request = new ReviewCreateRequest { Name = new string('a', 61), Rating = 5, Text = "Great work." };
        var errors = ReviewValidator.Validate(request);
        Assert.Contains(errors, e => e.Contains("Name"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Rating_outside_1_to_5_is_rejected(int rating)
    {
        var request = new ReviewCreateRequest { Name = "Priya", Rating = rating, Text = "Great work." };
        var errors = ReviewValidator.Validate(request);
        Assert.Contains(errors, e => e.Contains("Rating"));
    }

    [Fact]
    public void Empty_text_is_rejected()
    {
        var request = new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = "" };
        var errors = ReviewValidator.Validate(request);
        Assert.Contains(errors, e => e.Contains("Text"));
    }

    [Fact]
    public void Text_over_500_chars_is_rejected()
    {
        var request = new ReviewCreateRequest { Name = "Priya", Rating = 5, Text = new string('a', 501) };
        var errors = ReviewValidator.Validate(request);
        Assert.Contains(errors, e => e.Contains("Text"));
    }

    [Fact]
    public void Null_request_is_rejected_on_all_fields()
    {
        var errors = ReviewValidator.Validate(null);
        Assert.Equal(3, errors.Count);
    }
}
