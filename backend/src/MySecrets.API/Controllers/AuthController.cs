using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySecrets.Application.Auth.Commands;
using MySecrets.Application.Auth.DTOs;

namespace MySecrets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            GetClientIpAddress()
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(new ErrorResponse { Message = result.Error ?? "Registration failed" });
        }

        SetRefreshTokenCookie(result.Data!.RefreshToken);

        return CreatedAtAction(nameof(Register), new AuthResponseWithoutRefresh(result.Data));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            GetClientIpAddress()
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ErrorResponse { Message = result.Error ?? "Login failed" });
        }

        SetRefreshTokenCookie(result.Data!.RefreshToken);

        return Ok(new AuthResponseWithoutRefresh(result.Data));
    }

    /// <summary>
    /// Login with Google OAuth token
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
    {
        var command = new GoogleLoginCommand(
            request.IdToken,
            GetClientIpAddress()
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ErrorResponse { Message = result.Error ?? "Google login failed" });
        }

        SetRefreshTokenCookie(result.Data!.RefreshToken);

        return Ok(new AuthResponseWithoutRefresh(result.Data));
    }

    /// <summary>
    /// Refresh access token using refresh token cookie
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new ErrorResponse { Message = "No refresh token provided" });
        }

        var command = new RefreshTokenCommand(
            refreshToken,
            GetClientIpAddress()
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            // Clear invalid cookie
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(new ErrorResponse { Message = result.Error ?? "Token refresh failed" });
        }

        SetRefreshTokenCookie(result.Data!.RefreshToken);

        return Ok(new AuthResponseWithoutRefresh(result.Data));
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? "";

        var command = new LogoutCommand(
            refreshToken,
            GetClientIpAddress()
        );

        await _mediator.Send(command);

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// DTO without refresh token for response body (refresh token goes in cookie)
public record AuthResponseWithoutRefresh(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    DateTime AccessTokenExpiry
)
{
    public AuthResponseWithoutRefresh(AuthResponseDto dto) 
        : this(dto.UserId, dto.Email, dto.FirstName, dto.LastName, dto.AccessToken, dto.AccessTokenExpiry)
    {
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
