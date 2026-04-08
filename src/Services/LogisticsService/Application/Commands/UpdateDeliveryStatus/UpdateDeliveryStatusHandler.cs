using Artistic_Sisters.Shared.Events.Order;
using LogisticsService.Domain.Enums;
using LogisticsService.Infrastructure.Cache;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Commands.UpdateDeliveryStatus;

public class UpdateDeliveryStatusHandler
    : IRequestHandler<UpdateDeliveryStatusCommand, UpdateStatusResult>
{
    private readonly LogisticsDbContext _db;
    private readonly ILogisticsCache _cache;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<UpdateDeliveryStatusHandler> _logger;

    public UpdateDeliveryStatusHandler(
        LogisticsDbContext db,
        ILogisticsCache cache,
        IPublishEndpoint publisher,
        ILogger<UpdateDeliveryStatusHandler> logger)
    {
        _db = db;
        _cache = cache;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<UpdateStatusResult> Handle(
        UpdateDeliveryStatusCommand request, CancellationToken ct)
    {
        // Find the assignment
        var assignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.OrderId == request.OrderId, ct);

        if (assignment == null)
            return new UpdateStatusResult
            {
                Success = false,
                Message = "Assignment not found"
            };

        // Update the status
        assignment.Status = request.NewStatus;

        // If GPS coordinates provided — update them
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            assignment.LastLatitude = request.Latitude;
            assignment.LastLongitude = request.Longitude;
            assignment.LastGPSUpdate = DateTime.UtcNow;

            // Update Redis GPS cache — dealer tracking page reads this
            await _cache.SetGPSAsync(
                request.OrderId,
                request.Latitude.Value,
                request.Longitude.Value);
        }

        await _db.SaveChangesAsync(ct);

        // If delivered — publish DeliveryCompletedEvent
        // Order Service will close the order
        // Notification Service will email the dealer
        if (request.NewStatus == DeliveryStatus.Delivered)
        {
            await _publisher.Publish(new DeliveryCompletedEvent
            {
                OrderId = request.OrderId,
                DeliveredAt = DateTime.UtcNow
            });

            // Release agent and vehicle
            var agentAssignment = await _db.Assignments
                .Include(a => a.Agent)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.OrderId == request.OrderId, ct);

            if (agentAssignment != null)
            {
                agentAssignment.Agent.Release();
                agentAssignment.Vehicle.IsAvailable = true;
                await _db.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation(
            "Order {OrderId} status updated to {Status}",
            request.OrderId, request.NewStatus);

        return new UpdateStatusResult
        {
            Success = true,
            Message = $"Status updated to {request.NewStatus}"
        };
    }
}
