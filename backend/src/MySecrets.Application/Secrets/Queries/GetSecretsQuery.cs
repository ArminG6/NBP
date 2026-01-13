using MediatR;
using Microsoft.EntityFrameworkCore;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Application.Secrets.DTOs;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Queries;

public record GetSecretsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? Category = null,
    bool? IsFavorite = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<Result<SecretListResponseDto>>;

public class GetSecretsQueryHandler : IRequestHandler<GetSecretsQuery, Result<SecretListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSecretsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SecretListResponseDto>> Handle(
        GetSecretsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<SecretListResponseDto>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;
        var query = _unitOfWork.Secrets.GetQueryableByUserId(userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.WebsiteUrl.ToLower().Contains(searchTerm) ||
                s.Username.ToLower().Contains(searchTerm) ||
                (s.Notes != null && s.Notes.ToLower().Contains(searchTerm)));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        // Apply favorite filter
        if (request.IsFavorite.HasValue)
        {
            query = query.Where(s => s.IsFavorite == request.IsFavorite.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "websiteurl" => request.SortDescending
                ? query.OrderByDescending(s => s.WebsiteUrl)
                : query.OrderBy(s => s.WebsiteUrl),
            "username" => request.SortDescending
                ? query.OrderByDescending(s => s.Username)
                : query.OrderBy(s => s.Username),
            "category" => request.SortDescending
                ? query.OrderByDescending(s => s.Category)
                : query.OrderBy(s => s.Category),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt),
            "updatedat" => request.SortDescending
                ? query.OrderByDescending(s => s.UpdatedAt)
                : query.OrderBy(s => s.UpdatedAt),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var paginatedList = await PaginatedList<SecretDto>.CreateAsync(
            query.Select(s => new SecretDto(
                s.Id,
                s.WebsiteUrl,
                s.Username,
                s.Notes,
                s.Category,
                s.IsFavorite,
                s.CreatedAt,
                s.UpdatedAt
            )),
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        return Result<SecretListResponseDto>.Success(new SecretListResponseDto(
            paginatedList.Items,
            paginatedList.PageNumber,
            paginatedList.PageSize,
            paginatedList.TotalPages,
            paginatedList.TotalCount,
            paginatedList.HasPreviousPage,
            paginatedList.HasNextPage
        ));
    }
}
