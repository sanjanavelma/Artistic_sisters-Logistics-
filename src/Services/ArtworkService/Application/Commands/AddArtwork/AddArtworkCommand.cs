using MediatR;

namespace ArtworkService.Application.Commands.AddArtwork;

public record AddArtworkCommand : IRequest<AddArtworkResult>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string ArtworkType { get; init; } = string.Empty;
    public string Medium { get; init; } = string.Empty;
    public string Dimensions { get; init; } = string.Empty;
    public string ArtworkCode { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int AvailableQuantity { get; init; }
    public bool IsCustomizable { get; init; } = false;
    public int EstimatedCompletionDays { get; init; } = 7;
}
public record AddArtworkResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? ArtworkId { get; init; }
}
