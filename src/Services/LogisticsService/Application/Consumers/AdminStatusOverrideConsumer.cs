using Artistic_Sisters.Shared.Events.Order;
using LogisticsService.Domain.Enums;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Consumers;

/// <summary>
/// Listens for OrderStatusChangedEvent when UpdaterRole = "Admin".
/// Syncs the LogisticsDB Assignment status to match the Admin's decision.
/// Admin always wins — this overwrites whatever the Agent had set.
/// If Admin marks Delivered, agent and vehicle are released here too.
/// </summary>
public class AdminStatusOverrideConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly LogisticsDbContext _db;
    private readonly ILogger<AdminStatusOverrideConsumer> _logger;

    public AdminStatusOverrideConsumer(LogisticsDbContext db, ILogger<AdminStatusOverrideConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var evt = context.Message;

        // Only process Admin overrides
        if (!string.Equals(evt.UpdaterRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("AdminStatusOverrideConsumer: ignoring Agent update for order {OrderId}", evt.OrderId);
            return;
        }

        // Find assignment for this order (might not exist if order hasn't been dispatched yet)
        var assignment = await _db.Assignments
            .Include(a => a.Agent)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.OrderId == evt.OrderId);

        if (assignment == null)
        {
            _logger.LogInformation(
                "AdminStatusOverrideConsumer: No assignment found for Order {OrderId} — nothing to sync",
                evt.OrderId);
            return;
        }

        // Map the OrderStatus string to DeliveryStatus
        var newDeliveryStatus = MapToDeliveryStatus(evt.NewStatus);
        if (newDeliveryStatus == null)
        {
            _logger.LogWarning(
                "AdminStatusOverrideConsumer: Cannot map order status '{Status}' to DeliveryStatus — skipping",
                evt.NewStatus);
            return;
        }

        var oldStatus = assignment.Status;
        assignment.Status = newDeliveryStatus.Value;

        // If Admin marked as Delivered — release agent and vehicle
        if (newDeliveryStatus == DeliveryStatus.Delivered && oldStatus != DeliveryStatus.Delivered)
        {
            if (assignment.Agent != null) assignment.Agent.Release();
            if (assignment.Vehicle != null) assignment.Vehicle.IsAvailable = true;
        }

        // If Admin cancelled — also compensate
        if (newDeliveryStatus == DeliveryStatus.Cancelled && oldStatus != DeliveryStatus.Cancelled)
        {
            assignment.IsCompensated = true;
            if (assignment.Agent != null) assignment.Agent.Release();
            if (assignment.Vehicle != null) assignment.Vehicle.IsAvailable = true;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Admin override applied to Assignment for Order {OrderId}: {Old} → {New}",
            evt.OrderId, oldStatus, newDeliveryStatus);
    }

    /// <summary>
    /// Maps OrderStatus string from Admin to the closest DeliveryStatus.
    /// </summary>
    private static DeliveryStatus? MapToDeliveryStatus(string orderStatus) => orderStatus switch
    {
        "Delivered"        => DeliveryStatus.Delivered,
        "Cancelled"        => DeliveryStatus.Cancelled,
        "Dispatched"       => DeliveryStatus.PickedUp,
        "ReadyForDispatch" => null,   // no change needed — agent not involved yet
        "Processing"       => null,
        "Pending"          => null,
        _                  => null
    };
}
