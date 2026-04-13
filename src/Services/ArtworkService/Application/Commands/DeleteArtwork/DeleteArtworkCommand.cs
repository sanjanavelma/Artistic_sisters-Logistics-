using MediatR;

namespace ArtworkService.Application.Commands.DeleteArtwork;

public record DeleteArtworkCommand : IRequest<DeleteArtworkResult>
{
    public Guid ArtworkId { get; init; }
}

public record DeleteArtworkResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
