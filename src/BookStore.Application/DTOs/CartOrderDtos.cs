using BookStore.Domain.Enums;

namespace BookStore.Application.DTOs;

public sealed record CartItemDto(int Id, int BookId, string BookTitle, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record CartDto(int Id, IReadOnlyList<CartItemDto> Items, decimal TotalAmount);
public sealed record AddCartItemRequest(int BookId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);

public sealed record OrderItemDto(int Id, int BookId, string BookTitle, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderDto(int Id, string OrderNumber, decimal TotalAmount, OrderStatus Status, DateTime OrderedAt, IReadOnlyList<OrderItemDto> Items);
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
