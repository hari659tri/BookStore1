namespace BookStore.Application.DTOs;

public sealed record AuthorDto(int Id, string Name, string? Biography, bool IsActive);
public sealed record UpsertAuthorRequest(string Name, string? Biography, bool IsActive = true);

public sealed record CategoryDto(int Id, string Name, string? Description, bool IsActive);
public sealed record UpsertCategoryRequest(string Name, string? Description, bool IsActive = true);

public sealed record BookDto(
    int Id,
    string Title,
    string Isbn,
    decimal Price,
    int StockQuantity,
    string? Description,
    string? CoverImageUrl,
    int AuthorId,
    string AuthorName,
    int CategoryId,
    string CategoryName,
    bool IsActive);

public sealed record UpsertBookRequest(
    string Title,
    string Isbn,
    decimal Price,
    int StockQuantity,
    string? Description,
    string? CoverImageUrl,
    int AuthorId,
    int CategoryId,
    bool IsActive = true);
