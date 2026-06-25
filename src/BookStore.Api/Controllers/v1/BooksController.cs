using Asp.Versioning;
using BookStore.Application.Common;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/books")]
public sealed class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<BookDto>>> Get([FromQuery] BookQueryParameters parameters, CancellationToken cancellationToken)
    {
        return Ok(await _bookService.GetAsync(parameters, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _bookService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<ActionResult<BookDto>> Create(UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = book.Id, version = "1.0" }, book);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<ActionResult<BookDto>> Update(int id, UpsertBookRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _bookService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
