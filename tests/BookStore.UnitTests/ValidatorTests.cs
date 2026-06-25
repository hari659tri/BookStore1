using BookStore.Application.DTOs;
using BookStore.Application.Validators;

namespace BookStore.UnitTests;

public sealed class ValidatorTests
{
    [Fact]
    public void BookValidator_RejectsInvalidPrice()
    {
        var validator = new UpsertBookRequestValidator();
        var result = validator.Validate(new UpsertBookRequest("Clean Code", "9780132350884", 0, 10, null, null, 1, 1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Price");
    }

    [Fact]
    public void RegisterValidator_RequiresStrongPassword()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest("Demo", "User", "demo@example.com", "weak"));

        Assert.False(result.IsValid);
    }
}
