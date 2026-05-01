// Agent uses this to update status as they move through delivery
using LogisticsService.Domain.Enums;
using MediatR;

namespace LogisticsService.Application.Commands.UpdateDeliveryStatus;

public record UpdateDeliveryStatusCommand : IRequest<UpdateStatusResult>
{
    // Which order is being updated
    public Guid OrderId { get; init; }

    // New status — PickedUp, InTransit, OutForDelivery, Delivered
    public DeliveryStatus NewStatus { get; init; }

    // Optional note from agent — e.g. "Traffic delay"
    public string? Note { get; init; }
}

public record UpdateStatusResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
