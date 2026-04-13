using System;

namespace Artistic_Sisters.Shared.Events.Payment;

public record AssignAgentCommand
{
    public Guid OrderId { get; init; }
    public int SLAHours { get; init; } = 24;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerAddress { get; init; } = string.Empty;
}
