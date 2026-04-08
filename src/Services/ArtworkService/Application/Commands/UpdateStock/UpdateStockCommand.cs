using MediatR;

namespace ArtworkService.Application.Commands.UpdateStock;

public record UpdateStockCommand : IRequest<UpdateStockResult>
{
    public Guid ArtworkId { get; init; }
    public int Quantity { get; init; }
}
public record UpdateStockResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record UpdateStockRequest
{
    public int Quantity { get; init; }
}

