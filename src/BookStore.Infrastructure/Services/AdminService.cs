using BookStore.Application.Common;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Enums;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Services;

public sealed class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<Order> _orders;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(UserManager<ApplicationUser> userManager, IRepository<Order> orders, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _orders = orders;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking().OrderBy(x => x.Email).ToListAsync(cancellationToken);
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto(user.Id, user.Email ?? string.Empty, $"{user.FirstName} {user.LastName}".Trim(), roles.ToList(), user.IsActive));
        }

        return result;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _orders.Query().AsNoTracking()
            .OrderByDescending(order => order.OrderedAt)
            .Select(order => new OrderDto(
                order.Id,
                order.OrderNumber,
                order.TotalAmount,
                order.Status,
                order.OrderedAt,
                order.Items.Select(item => new OrderItemDto(item.Id, item.BookId, item.Book.Title, item.Quantity, item.UnitPrice, item.LineTotal)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _orders.Query().Include(x => x.Items).ThenInclude(x => x.Book).FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new AppException("Order not found.", 404);

        order.Status = status;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrderDto(order.Id, order.OrderNumber, order.TotalAmount, order.Status, order.OrderedAt,
            order.Items.Select(item => new OrderItemDto(item.Id, item.BookId, item.Book.Title, item.Quantity, item.UnitPrice, item.LineTotal)).ToList());
    }
}
