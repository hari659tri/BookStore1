using BookStore.Application.Common;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Services;

public sealed class CartService : ICartService
{
    private readonly IRepository<Cart> _carts;
    private readonly IRepository<CartItem> _cartItems;
    private readonly IRepository<Book> _books;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IRepository<Cart> carts, IRepository<CartItem> cartItems, IRepository<Book> books, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _cartItems = cartItems;
        _books = books;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return await BuildCartDtoAsync(cart.Id, cancellationToken);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var book = await _books.Query().FirstOrDefaultAsync(x => x.Id == request.BookId && x.IsActive, cancellationToken)
            ?? throw new AppException("Book not found.", 404);

        if (book.StockQuantity < request.Quantity)
        {
            throw new AppException("Requested quantity is not available.", 409);
        }

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = await _cartItems.FirstOrDefaultAsync(x => x.CartId == cart.Id && x.BookId == request.BookId, cancellationToken);

        if (item is null)
        {
            await _cartItems.AddAsync(new CartItem
            {
                CartId = cart.Id,
                BookId = book.Id,
                Quantity = request.Quantity,
                UnitPrice = book.Price
            }, cancellationToken);
        }
        else
        {
            var newQuantity = item.Quantity + request.Quantity;
            if (book.StockQuantity < newQuantity)
            {
                throw new AppException("Requested quantity is not available.", 409);
            }

            item.Quantity = newQuantity;
            item.UnitPrice = book.Price;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cart.Id, cancellationToken);
    }

    public async Task<CartDto> UpdateItemAsync(string userId, int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = await _cartItems.Query()
            .Include(x => x.Book)
            .FirstOrDefaultAsync(x => x.Id == itemId && x.CartId == cart.Id, cancellationToken)
            ?? throw new AppException("Cart item not found.", 404);

        if (item.Book.StockQuantity < request.Quantity)
        {
            throw new AppException("Requested quantity is not available.", 409);
        }

        item.Quantity = request.Quantity;
        item.UnitPrice = item.Book.Price;
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cart.Id, cancellationToken);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, int itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var item = await _cartItems.FirstOrDefaultAsync(x => x.Id == itemId && x.CartId == cart.Id, cancellationToken)
            ?? throw new AppException("Cart item not found.", 404);

        _cartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await BuildCartDtoAsync(cart.Id, cancellationToken);
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await _carts.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        await _carts.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private async Task<CartDto> BuildCartDtoAsync(int cartId, CancellationToken cancellationToken)
    {
        var items = await _cartItems.Query()
            .AsNoTracking()
            .Where(x => x.CartId == cartId)
            .Select(x => new CartItemDto(x.Id, x.BookId, x.Book.Title, x.Quantity, x.UnitPrice, x.Quantity * x.UnitPrice))
            .ToListAsync(cancellationToken);

        return new CartDto(cartId, items, items.Sum(x => x.LineTotal));
    }
}

public sealed class OrderService : IOrderService
{
    private readonly IRepository<Cart> _carts;
    private readonly IRepository<CartItem> _cartItems;
    private readonly IRepository<Order> _orders;
    private readonly IRepository<Book> _books;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IRepository<Cart> carts, IRepository<CartItem> cartItems, IRepository<Order> orders, IRepository<Book> books, IUnitOfWork unitOfWork)
    {
        _carts = carts;
        _cartItems = cartItems;
        _orders = orders;
        _books = books;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> PlaceOrderAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var cart = await _carts.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
                ?? throw new AppException("Cart is empty.", 400);

            var items = await _cartItems.Query()
                .Include(x => x.Book)
                .Where(x => x.CartId == cart.Id)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                throw new AppException("Cart is empty.", 400);
            }

            foreach (var item in items)
            {
                if (!item.Book.IsActive || item.Book.StockQuantity < item.Quantity)
                {
                    throw new AppException($"Insufficient stock for {item.Book.Title}.", 409);
                }

                item.Book.StockQuantity -= item.Quantity;
            }

            var order = new Order
            {
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                Status = OrderStatus.Confirmed,
                OrderedAt = DateTime.UtcNow,
                Items = items.Select(item => new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                }).ToList()
            };
            order.TotalAmount = order.Items.Sum(x => x.LineTotal);

            await _orders.AddAsync(order, cancellationToken);
            foreach (var item in items)
            {
                _cartItems.Remove(item);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await BuildOrderDtoAsync(order.Id, userId, cancellationToken);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderDto>> GetHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _orders.Query()
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OrderedAt)
            .Select(order => new OrderDto(
                order.Id,
                order.OrderNumber,
                order.TotalAmount,
                order.Status,
                order.OrderedAt,
                order.Items.Select(item => new OrderItemDto(item.Id, item.BookId, item.Book.Title, item.Quantity, item.UnitPrice, item.LineTotal)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto> GetByIdAsync(string userId, int orderId, CancellationToken cancellationToken = default)
    {
        return await BuildOrderDtoAsync(orderId, userId, cancellationToken);
    }

    private async Task<OrderDto> BuildOrderDtoAsync(int orderId, string userId, CancellationToken cancellationToken)
    {
        var order = await _orders.Query()
            .AsNoTracking()
            .Where(x => x.Id == orderId && x.UserId == userId)
            .Select(x => new OrderDto(
                x.Id,
                x.OrderNumber,
                x.TotalAmount,
                x.Status,
                x.OrderedAt,
                x.Items.Select(item => new OrderItemDto(item.Id, item.BookId, item.Book.Title, item.Quantity, item.UnitPrice, item.LineTotal)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return order ?? throw new AppException("Order not found.", 404);
    }
}
