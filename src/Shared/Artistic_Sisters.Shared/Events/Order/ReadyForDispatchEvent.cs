using System;

namespace Artistic_Sisters.Shared.Events.Order;

public record ReadyForDispatchEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerAddress { get; init; } = string.Empty;
    public DateTime ReadyAt { get; init; }
    public int SLAHours { get; init; } = 24;
}
