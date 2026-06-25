using Asp.Versioning;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/authors")]
public sealed class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<AuthorDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _authorService.GetAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await _authorService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<ActionResult<AuthorDto>> Create(UpsertAuthorRequest request, CancellationToken cancellationToken)
    {
        var author = await _authorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = author.Id, version = "1.0" }, author);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<ActionResult<AuthorDto>> Update(int id, UpsertAuthorRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _authorService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = SystemRoles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _authorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
