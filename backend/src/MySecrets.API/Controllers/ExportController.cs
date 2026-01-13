using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySecrets.Application.Export.DTOs;
using MySecrets.Application.Export.Queries;

namespace MySecrets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Export secrets to CSV format (passwords remain encrypted)
    /// </summary>
    [HttpGet("csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCsv()
    {
        var query = new ExportSecretsQuery(ExportFormat.Csv);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return File(result.Data!.Content, result.Data.ContentType, result.Data.FileName);
    }

    /// <summary>
    /// Export secrets to TXT format (passwords remain encrypted)
    /// </summary>
    [HttpGet("txt")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTxt()
    {
        var query = new ExportSecretsQuery(ExportFormat.Txt);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return File(result.Data!.Content, result.Data.ContentType, result.Data.FileName);
    }
}
