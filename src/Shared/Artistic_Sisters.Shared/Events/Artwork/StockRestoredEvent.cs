using System;

namespace Artistic_Sisters.Shared.Events.Artwork;

public record StockRestoredEvent
{
    public Guid ArtworkId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int QuantityRestored { get; init; }
    public DateTime RestoredAt { get; init; }
}
