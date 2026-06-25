using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookStore.IntegrationTests;

public sealed class ApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_IsAvailableInDevelopment()
    {
        using var client = _factory.WithWebHostBuilder(_ => { }).CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
