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
        // ── Load assignment with all nav props ────────────────────────────────
        var assignment = await _db.Assignments
            .Include(a => a.Agent)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.OrderId == request.OrderId, ct);

        if (assignment == null)
            return new UpdateStatusResult { Success = false, Message = "Assignment not found" };

        var oldStatus = assignment.Status;
        assignment.Status = request.NewStatus;

        // ── GPS update ────────────────────────────────────────────────────────
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            assignment.LastLatitude = request.Latitude;
            assignment.LastLongitude = request.Longitude;
            assignment.LastGPSUpdate = DateTime.UtcNow;

            await _cache.SetGPSAsync(
                request.OrderId,
                request.Latitude.Value,
                request.Longitude.Value);
        }

        await _db.SaveChangesAsync(ct);

        // ── If Delivered — release agent & vehicle ────────────────────────────
        if (request.NewStatus == DeliveryStatus.Delivered)
        {
            assignment.Agent.Release();
            assignment.Vehicle.IsAvailable = true;
            await _db.SaveChangesAsync(ct);

            // Also publish the original DeliveryCompletedEvent so existing
            // NotificationService.DeliveryCompletedConsumer still fires correctly
            await _publisher.Publish(new DeliveryCompletedEvent
            {
                OrderId = request.OrderId,
                CustomerId = Guid.Empty,  // not stored on assignment; Order Service has it
                CustomerName = assignment.CustomerName,
                CustomerEmail = assignment.CustomerEmail,
                DeliveredAt = DateTime.UtcNow
            }, ct);
        }

        // ── Publish OrderStatusChangedEvent:                                ──
        //   → OrderService.DeliveryStatusSyncConsumer syncs Order DB
        //   → NotificationService.OrderStatusChangedConsumer emails everyone
        await _publisher.Publish(new OrderStatusChangedEvent
        {
            OrderId = request.OrderId,
            UpdaterRole = "Agent",
            NewStatus = request.NewStatus.ToString(),
            OldStatus = oldStatus.ToString(),
            CustomerName = assignment.CustomerName,
            CustomerEmail = assignment.CustomerEmail,
            AgentName = assignment.Agent?.Name ?? "",
            AgentEmail = assignment.Agent?.Email ?? "",
            ChangedAt = DateTime.UtcNow
        }, ct);

        _logger.LogInformation(
            "Agent updated Order {OrderId} delivery: {Old} → {New}",
            request.OrderId, oldStatus, request.NewStatus);

        return new UpdateStatusResult
        {
            Success = true,
            Message = $"Status updated to {request.NewStatus}"
        };
    }
}
