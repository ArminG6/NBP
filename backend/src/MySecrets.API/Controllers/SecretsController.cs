using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySecrets.Application.Secrets.Commands;
using MySecrets.Application.Secrets.DTOs;
using MySecrets.Application.Secrets.Queries;

namespace MySecrets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecretsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SecretsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of secrets with optional filtering and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SecretListResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecrets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isFavorite = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetSecretsQuery(
            pageNumber,
            pageSize,
            searchTerm,
            category,
            isFavorite,
            sortBy,
            sortDescending
        );

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get a specific secret by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SecretDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSecret(Guid id)
    {
        var query = new GetSecretByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Decrypt and reveal a secret's password (explicit user action required)
    /// </summary>
    [HttpGet("{id:guid}/decrypt")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecryptPassword(Guid id)
    {
        var query = new DecryptPasswordQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(new { password = result.Data });
    }

    /// <summary>
    /// Create a new secret
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SecretDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSecret([FromBody] CreateSecretDto request)
    {
        var command = new CreateSecretCommand(
            request.WebsiteUrl,
            request.Username,
            request.Password,
            request.Notes,
            request.Category,
            request.IsFavorite
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetSecret), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Update an existing secret
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SecretDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSecret(Guid id, [FromBody] UpdateSecretDto request)
    {
        var command = new UpdateSecretCommand(
            id,
            request.WebsiteUrl,
            request.Username,
            request.Password,
            request.Notes,
            request.Category,
            request.IsFavorite
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error == "Secret not found")
            {
                return NotFound(new { message = result.Error });
            }
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a secret
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSecret(Guid id)
    {
        var command = new DeleteSecretCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Error });
        }

        return NoContent();
    }
}
