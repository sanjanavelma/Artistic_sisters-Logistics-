using ArtworkService.Infrastructure.Cache;
using ArtworkService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtworkService.Application.Commands.ToggleComingSoon;

public class ToggleComingSoonHandler : IRequestHandler<ToggleComingSoonCommand, ToggleComingSoonResult>
{
    private readonly ArtworkDbContext _db;
    private readonly IArtworkCache _cache;

    public ToggleComingSoonHandler(ArtworkDbContext db, IArtworkCache cache)
    {
        _db = db; _cache = cache;
    }

    public async Task<ToggleComingSoonResult> Handle(ToggleComingSoonCommand request, CancellationToken ct)
    {
        var art = await _db.Artworks.FirstOrDefaultAsync(a => a.Id == request.ArtworkId && a.IsActive, ct);
        if (art == null)
            return new ToggleComingSoonResult { Success = false, Message = "Artwork not found or has been deleted" };

        art.IsComingSoon = request.IsComingSoon;
        art.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await _cache.RemoveByPatternAsync("artworks:*");

        var msg = request.IsComingSoon
            ? $"'{art.Title}' is now marked as Coming Soon"
            : $"'{art.Title}' is now available again";

        return new ToggleComingSoonResult { Success = true, Message = msg };
    }
}
