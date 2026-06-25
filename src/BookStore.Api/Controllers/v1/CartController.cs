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
[Route("api/v{version:apiVersion}/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _cartService.GetAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _cartService.AddItemAsync(User.GetUserId(), request, cancellationToken));
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int itemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _cartService.UpdateItemAsync(User.GetUserId(), itemId, request, cancellationToken));
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int itemId, CancellationToken cancellationToken)
    {
        return Ok(await _cartService.RemoveItemAsync(User.GetUserId(), itemId, cancellationToken));
    }
}
