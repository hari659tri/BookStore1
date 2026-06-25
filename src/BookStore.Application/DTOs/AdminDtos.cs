namespace BookStore.Application.DTOs;

public sealed record UserDto(string Id, string Email, string FullName, IReadOnlyList<string> Roles, bool IsActive);
