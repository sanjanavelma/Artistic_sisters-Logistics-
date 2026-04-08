using System;

namespace Artistic_Sisters.Shared.Events.Order;

public record CustomCommissionPlacedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string ArtworkType { get; init; } = string.Empty;
    public string Medium { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string ReferencePhotoUrl { get; init; } = string.Empty;
    public string SpecialInstructions { get; init; } = string.Empty;
    public decimal BudgetMin { get; init; }
    public decimal BudgetMax { get; init; }
    public DateTime PlacedAt { get; init; }
}
