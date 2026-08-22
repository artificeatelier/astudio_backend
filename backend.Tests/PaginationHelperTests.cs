using System.Collections.Generic;
using backend.Services;
using Xunit;

public class PaginationHelperTests
{
    [Fact]
    public void ClampLimit_defaults_to_6_when_null()
    {
        Assert.Equal(6, PaginationHelper.ClampLimit(null));
    }

    [Fact]
    public void ClampLimit_defaults_to_6_when_zero_or_negative()
    {
        Assert.Equal(6, PaginationHelper.ClampLimit(0));
        Assert.Equal(6, PaginationHelper.ClampLimit(-5));
    }

    [Fact]
    public void ClampLimit_caps_at_24()
    {
        Assert.Equal(24, PaginationHelper.ClampLimit(1000));
    }

    [Fact]
    public void ClampLimit_passes_through_valid_values()
    {
        Assert.Equal(10, PaginationHelper.ClampLimit(10));
    }

    [Fact]
    public void ClampSkip_defaults_to_0_when_null_or_negative()
    {
        Assert.Equal(0, PaginationHelper.ClampSkip(null));
        Assert.Equal(0, PaginationHelper.ClampSkip(-1));
    }

    [Fact]
    public void ClampSkip_passes_through_valid_values()
    {
        Assert.Equal(12, PaginationHelper.ClampSkip(12));
    }

    [Fact]
    public void TrimForPage_reports_no_more_when_fetched_equals_limit()
    {
        var fetched = new List<int> { 1, 2, 3 };
        var (items, hasMore) = PaginationHelper.TrimForPage(fetched, 3);
        Assert.Equal(3, items.Count);
        Assert.False(hasMore);
    }

    [Fact]
    public void TrimForPage_reports_has_more_and_trims_extra_item()
    {
        var fetched = new List<int> { 1, 2, 3, 4 };
        var (items, hasMore) = PaginationHelper.TrimForPage(fetched, 3);
        Assert.Equal(new List<int> { 1, 2, 3 }, items);
        Assert.True(hasMore);
    }
}
