namespace BookStore.Application.DTOs;

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string UserId, string Email, string FullName, IReadOnlyList<string> Roles, string Token, DateTime ExpiresAt);
