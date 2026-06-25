using Asp.Versioning;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = SystemRoles.Admin)]
[Route("api/v{version:apiVersion}/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _adminService.GetUsersAsync(cancellationToken));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(CancellationToken cancellationToken)
    {
        return Ok(await _adminService.GetOrdersAsync(cancellationToken));
    }

    [HttpPut("orders/{orderId:int}/status")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminService.UpdateOrderStatusAsync(orderId, request.Status, cancellationToken));
    }
}
