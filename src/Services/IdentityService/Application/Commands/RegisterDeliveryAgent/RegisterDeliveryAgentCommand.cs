using MediatR;

namespace IdentityService.Application.Commands.RegisterDeliveryAgent;

public record RegisterDeliveryAgentCommand : IRequest<RegisterDeliveryAgentResult>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}

public record RegisterDeliveryAgentResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? AgentId { get; init; }
    public string? Token { get; init; }
    public string? Role { get; init; }
    public string? Name { get; init; }
}
