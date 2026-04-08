using System;

namespace Artistic_Sisters.Shared.Events.Logistics;

public record AgentAssignedEvent
{
    public Guid OrderId { get; init; }
    public Guid AssignmentId { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public string AgentPhone { get; init; } = string.Empty;
    public string VehicleNumber { get; init; } = string.Empty;
    public DateTime SLADeadline { get; init; }
}
