using System;

namespace Artistic_Sisters.Shared.Events.Payment;

public record DispatchConfirmedEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string AgentName { get; init; } = string.Empty;
    public string AgentPhone { get; init; } = string.Empty;
    public string VehicleNumber { get; init; } = string.Empty;
    public DateTime DispatchedAt { get; init; }
}
