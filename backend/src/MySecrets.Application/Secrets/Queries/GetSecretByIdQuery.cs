using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Application.Secrets.DTOs;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Queries;

public record GetSecretByIdQuery(Guid Id) : IRequest<Result<SecretDetailDto>>;

public class GetSecretByIdQueryHandler : IRequestHandler<GetSecretByIdQuery, Result<SecretDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSecretByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SecretDetailDto>> Handle(
        GetSecretByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<SecretDetailDto>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        var secret = await _unitOfWork.Secrets.GetByIdAndUserIdAsync(
            request.Id, userId, cancellationToken);

        if (secret == null)
        {
            return Result<SecretDetailDto>.Failure("Secret not found");
        }

        return Result<SecretDetailDto>.Success(new SecretDetailDto(
            secret.Id,
            secret.WebsiteUrl,
            secret.Username,
            secret.Notes,
            secret.Category,
            secret.IsFavorite,
            secret.CreatedAt,
            secret.UpdatedAt
        ));
    }
}
