using System;

namespace Artistic_Sisters.Shared.Events.Logistics;

public record AssignAgentFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; }
}
