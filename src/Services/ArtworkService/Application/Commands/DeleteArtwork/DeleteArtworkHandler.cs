using ArtworkService.Infrastructure.Cache;
using ArtworkService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtworkService.Application.Commands.DeleteArtwork;

public class DeleteArtworkHandler : IRequestHandler<DeleteArtworkCommand, DeleteArtworkResult>
{
    private readonly ArtworkDbContext _db;
    private readonly IArtworkCache _cache;

    public DeleteArtworkHandler(ArtworkDbContext db, IArtworkCache cache)
    {
        _db = db; _cache = cache;
    }

    public async Task<DeleteArtworkResult> Handle(DeleteArtworkCommand request, CancellationToken ct)
    {
        var art = await _db.Artworks.FirstOrDefaultAsync(a => a.Id == request.ArtworkId, ct);
        if (art == null)
            return new DeleteArtworkResult { Success = false, Message = "Artwork not found" };

        // Soft delete — keeps data intact for historical orders
        art.IsActive = false;
        art.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Invalidate cache so the gallery refreshes for all users
        await _cache.RemoveByPatternAsync("artworks:*");

        return new DeleteArtworkResult { Success = true, Message = $"'{art.Title}' has been removed from the gallery" };
    }
}
