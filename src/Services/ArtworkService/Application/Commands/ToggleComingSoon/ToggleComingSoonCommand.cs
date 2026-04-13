using MediatR;

namespace ArtworkService.Application.Commands.ToggleComingSoon;

public record ToggleComingSoonCommand : IRequest<ToggleComingSoonResult>
{
    public Guid ArtworkId { get; init; }
    /// <summary>When true, artwork shows as "Coming Soon" (temporarily unavailable). When false, restores normal availability.</summary>
    public bool IsComingSoon { get; init; }
}

public record ToggleComingSoonResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
