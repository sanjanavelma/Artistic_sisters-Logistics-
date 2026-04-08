using MediatR;

namespace IdentityService.Application.Commands.Login;

public record LoginCommand : IRequest<LoginResult>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
public record LoginResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Token { get; init; }
    public string? Role { get; init; }
    public string? Name { get; init; }
    public Guid? CustomerId { get; init; }
}
