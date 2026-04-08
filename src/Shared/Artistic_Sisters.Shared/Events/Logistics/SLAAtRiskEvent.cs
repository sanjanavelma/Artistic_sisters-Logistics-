using System;

namespace Artistic_Sisters.Shared.Events.Logistics;

public record SLAAtRiskEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime SLADeadline { get; init; }
    public int MinutesRemaining { get; init; }
    public double RemainingPercent { get; init; }
}
