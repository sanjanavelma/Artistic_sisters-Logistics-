using MediatR;
using ArtworkService.Infrastructure.Cache;
using ArtworkService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ArtworkService.Application.Queries.GetAllArtworks;

public record GetAllArtworksQuery : IRequest<List<ArtworkDto>>
{
    public string? ArtworkType { get; init; }
    public string? Medium { get; init; }
}
public record ArtworkDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string ArtworkType { get; init; } = string.Empty;
    public string Medium { get; init; } = string.Empty;
    public string Dimensions { get; init; } = string.Empty;
    public string ArtworkCode { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int AvailableQuantity { get; init; }
    public bool IsCustomizable { get; init; }
    public bool IsAvailable { get; init; }
    public int EstimatedCompletionDays { get; init; }
}
public class GetAllArtworksHandler
    : IRequestHandler<GetAllArtworksQuery, List<ArtworkDto>>
{
    private readonly ArtworkDbContext _db;
    private readonly IArtworkCache _cache;
    public GetAllArtworksHandler(ArtworkDbContext db, IArtworkCache cache)
    { _db = db; _cache = cache; }
    public async Task<List<ArtworkDto>> Handle(
        GetAllArtworksQuery request, CancellationToken ct)
    {
        var cacheKey = $"artworks:type:{request.ArtworkType ?? "all"}:" +
                       $"medium:{request.Medium ?? "all"}";
        var cached = await _cache.GetAsync<List<ArtworkDto>>(cacheKey);
        if (cached != null) return cached;
        var query = _db.Artworks.Where(a => a.IsActive).AsQueryable();
        if (!string.IsNullOrEmpty(request.ArtworkType))
            query = query.Where(a => a.ArtworkType == request.ArtworkType);
        if (!string.IsNullOrEmpty(request.Medium))
            query = query.Where(a => a.Medium == request.Medium);
        var artworks = await query.OrderBy(a => a.Title)
            .Select(a => new ArtworkDto
            {
                Id = a.Id, Title = a.Title,
                Description = a.Description, Price = a.Price,
                ArtworkType = a.ArtworkType, Medium = a.Medium,
                Dimensions = a.Dimensions, ArtworkCode = a.ArtworkCode,
                ImageUrl = a.ImageUrl,
                AvailableQuantity = a.AvailableQuantity,
                IsCustomizable = a.IsCustomizable,
                IsAvailable = a.AvailableQuantity > 0,
                EstimatedCompletionDays = a.EstimatedCompletionDays
            }).ToListAsync(ct);
        await _cache.SetAsync(cacheKey, artworks,
            TimeSpan.FromMinutes(30));
        return artworks;
    }
}
