using System;

namespace Artistic_Sisters.Shared.Events.Identity;

public record CustomerApprovedEvent
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime ApprovedAt { get; init; }
}
