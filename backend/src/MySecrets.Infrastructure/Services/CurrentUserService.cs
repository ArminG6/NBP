using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MySecrets.Application.Common.Interfaces;

namespace MySecrets.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            }

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ??
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
