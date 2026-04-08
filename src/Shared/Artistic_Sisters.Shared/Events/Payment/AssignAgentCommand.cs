using System;

namespace Artistic_Sisters.Shared.Events.Payment;

public record AssignAgentCommand
{
    public Guid OrderId { get; init; }
    public int SLAHours { get; init; } = 24;
}
