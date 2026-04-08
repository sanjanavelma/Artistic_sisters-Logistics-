using System;

namespace Artistic_Sisters.Shared.Events.Order;

public record DeliveryCompletedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime DeliveredAt { get; init; }
}
