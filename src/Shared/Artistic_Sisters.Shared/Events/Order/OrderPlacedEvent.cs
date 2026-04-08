using System;
using System.Collections.Generic;

namespace Artistic_Sisters.Shared.Events.Order;

public record OrderPlacedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string OrderType { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime PlacedAt { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

public record OrderItemDto
{
    public Guid ArtworkId { get; init; }
    public string ArtworkTitle { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
