using Asp.Versioning;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = SystemRoles.Customer)]
[Route("api/v{version:apiVersion}/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> PlaceOrder(CancellationToken cancellationToken)
    {
        return Ok(await _orderService.PlaceOrderAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetHistory(CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetHistoryAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetByIdAsync(User.GetUserId(), id, cancellationToken));
    }
}
