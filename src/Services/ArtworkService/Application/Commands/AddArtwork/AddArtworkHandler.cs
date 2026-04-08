using ArtworkService.Domain.Entities;
using ArtworkService.Infrastructure.Cache;
using ArtworkService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ArtworkService.Application.Commands.AddArtwork;
public class AddArtworkHandler
    : IRequestHandler<AddArtworkCommand, AddArtworkResult>
{
    private readonly ArtworkDbContext _db;
    private readonly IArtworkCache _cache;
    public AddArtworkHandler(ArtworkDbContext db, IArtworkCache cache)
    { _db = db; _cache = cache; }
    public async Task<AddArtworkResult> Handle(
        AddArtworkCommand request, CancellationToken ct)
    {
        var exists = await _db.Artworks
            .AnyAsync(a => a.ArtworkCode == request.ArtworkCode, ct);
        if (exists)
            return new AddArtworkResult
            { Success = false,
              Message = $"Artwork code {request.ArtworkCode} already exists" };
        var artwork = new Artwork
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            ArtworkType = request.ArtworkType,
            Medium = request.Medium,
            Dimensions = request.Dimensions,
            ArtworkCode = request.ArtworkCode,
            ImageUrl = request.ImageUrl,
            AvailableQuantity = request.AvailableQuantity,
            IsCustomizable = request.IsCustomizable,
            EstimatedCompletionDays = request.EstimatedCompletionDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Artworks.Add(artwork);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveByPatternAsync("artworks:*");
        return new AddArtworkResult
        { Success = true, Message = "Artwork added", ArtworkId = artwork.Id };
    }
}
