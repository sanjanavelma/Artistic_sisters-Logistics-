using System;

namespace Artistic_Sisters.Shared.Events.Order;

public record ReadyForDispatchEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime ReadyAt { get; init; }
    public int SLAHours { get; init; } = 24;
}
