using BookStore.Application.Common;

namespace BookStore.UnitTests;

public sealed class QueryParameterTests
{
    [Fact]
    public void PageValues_AreClampedToSafeDefaults()
    {
        var parameters = new BookQueryParameters
        {
            PageNumber = -10,
            PageSize = 500
        };

        Assert.Equal(1, parameters.PageNumber);
        Assert.Equal(10, parameters.PageSize);
    }
}
