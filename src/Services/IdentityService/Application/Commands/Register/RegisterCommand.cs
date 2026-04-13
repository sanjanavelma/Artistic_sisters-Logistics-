using MediatR;

namespace IdentityService.Application.Commands.Register;

public record RegisterCommand : IRequest<RegisterResult>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}
public record RegisterResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? CustomerId { get; init; }
    public string? Token { get; init; }
    public string? Email { get; init; }
}
