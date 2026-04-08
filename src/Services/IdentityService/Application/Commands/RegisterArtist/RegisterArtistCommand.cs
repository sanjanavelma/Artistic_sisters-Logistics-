using MediatR;

namespace IdentityService.Application.Commands.RegisterArtist;

public record RegisterArtistCommand : IRequest<RegisterArtistResult>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}

public record RegisterArtistResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid? CustomerId { get; init; }
    public string? Token { get; init; }
}
