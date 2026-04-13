using ArtworkService.Infrastructure.Cache;
using ArtworkService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtworkService.Application.Commands.UpdateStock;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, UpdateStockResult>
{
    private readonly ArtworkDbContext _db;
    private readonly IArtworkCache _cache;
    public UpdateStockHandler(ArtworkDbContext db, IArtworkCache cache) { _db = db; _cache = cache; }
    public async Task<UpdateStockResult> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var art = await _db.Artworks.FirstOrDefaultAsync(a => a.Id == request.ArtworkId, ct);
        if (art == null) return new UpdateStockResult { Success = false, Message = "Artwork not found" };
        
        // Fix 1: Assignment instead of +=
        art.AvailableQuantity = request.Quantity;
        art.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        
        // Fix 2: Cache invalidation
        await _cache.RemoveByPatternAsync("artworks:*");
        
        // Fix 3: Proper success message
        return new UpdateStockResult { Success = true, Message = $"Stock correctly updated to {request.Quantity}" };
    }
}
