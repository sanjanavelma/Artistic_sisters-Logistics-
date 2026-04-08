using System;

namespace Artistic_Sisters.Shared.Events.Identity;

public record CustomerRegisteredEvent
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
