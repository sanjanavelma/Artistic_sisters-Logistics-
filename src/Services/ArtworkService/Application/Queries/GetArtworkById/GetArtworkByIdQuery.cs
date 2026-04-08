using MediatR;
using ArtworkService.Domain.Entities;
using ArtworkService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArtworkService.Application.Queries.GetArtworkById;

public record GetArtworkByIdQuery(Guid ArtworkId) : IRequest<Artwork?>;
public class GetArtworkByIdHandler : IRequestHandler<GetArtworkByIdQuery, Artwork?>
{
    private readonly ArtworkDbContext _db;
    public GetArtworkByIdHandler(ArtworkDbContext db) { _db = db; }
    public async Task<Artwork?> Handle(GetArtworkByIdQuery request, CancellationToken ct)
    {
        return await _db.Artworks.FirstOrDefaultAsync(a => a.Id == request.ArtworkId, ct);
    }
}
