using BookStore.Application.Common;
using BookStore.Application.DTOs;
using BookStore.Domain.Enums;

namespace BookStore.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public interface IBookService
{
    Task<PagedResult<BookDto>> GetAsync(BookQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<BookDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookDto> CreateAsync(UpsertBookRequest request, CancellationToken cancellationToken = default);
    Task<BookDto> UpdateAsync(int id, UpsertBookRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAuthorService
{
    Task<IReadOnlyList<AuthorDto>> GetAsync(CancellationToken cancellationToken = default);
    Task<AuthorDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AuthorDto> CreateAsync(UpsertAuthorRequest request, CancellationToken cancellationToken = default);
    Task<AuthorDto> UpdateAsync(int id, UpsertAuthorRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, UpsertCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface ICartService
{
    Task<CartDto> GetAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateItemAsync(string userId, int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(string userId, int itemId, CancellationToken cancellationToken = default);
}

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetHistoryAsync(string userId, CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(string userId, int orderId, CancellationToken cancellationToken = default);
}

public interface IAdminService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
