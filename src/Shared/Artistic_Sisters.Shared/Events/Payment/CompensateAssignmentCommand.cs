using System;

namespace Artistic_Sisters.Shared.Events.Payment;

public record CompensateAssignmentCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
