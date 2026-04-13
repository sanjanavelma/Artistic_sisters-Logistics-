using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Consumers;

/// <summary>
/// Listens for OrderStatusChangedEvent published by the Delivery Agent (UpdaterRole = "Agent").
/// Updates the Order DB so it stays in sync with Logistics.
/// Admin changes are ignored here because Admin already wrote the Order DB directly.
/// </summary>
public class DeliveryStatusSyncConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly OrderDbContext _db;
    private readonly ILogger<DeliveryStatusSyncConsumer> _logger;

    public DeliveryStatusSyncConsumer(OrderDbContext db, ILogger<DeliveryStatusSyncConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var evt = context.Message;

        // Only process updates from Agents — Admin already wrote to Order DB directly
        if (!string.Equals(evt.UpdaterRole, "Agent", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("DeliveryStatusSyncConsumer: ignoring Admin update for order {OrderId}", evt.OrderId);
            return;
        }

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == evt.OrderId);
        if (order == null)
        {
            _logger.LogWarning("DeliveryStatusSyncConsumer: Order {OrderId} not found in OrderDB", evt.OrderId);
            return;
        }

        // Map logistics status string to OrderStatus
        var newOrderStatus = MapToOrderStatus(evt.NewStatus);
        if (newOrderStatus == null)
        {
            _logger.LogWarning(
                "DeliveryStatusSyncConsumer: Cannot map logistics status '{Status}' to OrderStatus — skipping",
                evt.NewStatus);
            return;
        }

        // Only update if it actually differs (avoids needless writes)
        if (order.Status != newOrderStatus.Value)
        {
            order.Status = newOrderStatus.Value;
            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "Order {OrderId} synced from Agent update: {Old} → {New}",
                evt.OrderId, evt.OldStatus, evt.NewStatus);
        }
    }

    /// <summary>
    /// Maps Logistics DeliveryStatus string values to OrderService OrderStatus values.
    /// PickedUp / InTransit / OutForDelivery all map to Dispatched.
    /// </summary>
    private static OrderStatus? MapToOrderStatus(string deliveryStatus) => deliveryStatus switch
    {
        "PickedUp"       => OrderStatus.ReadyForDelivery,
        "InTransit"      => OrderStatus.ReadyForDelivery,
        "OutForDelivery" => OrderStatus.OutForDelivery,
        "Delivered"      => OrderStatus.Delivered,
        "Failed"         => OrderStatus.ReadyForDelivery,   // admin can decide
        "Cancelled"      => OrderStatus.Cancelled,
        
        "ReadyForDelivery" => OrderStatus.ReadyForDelivery,
        "InProduction"   => OrderStatus.InProduction,
        "Confirmed"      => OrderStatus.Confirmed,
        
        // Accepted legacy updates
        "Dispatched"     => OrderStatus.ReadyForDelivery,
        "ReadyForDispatch" => OrderStatus.ReadyForDelivery,
        "Processing"     => OrderStatus.InProduction,
        "Pending"        => OrderStatus.Pending,
        _                => null
    };
}
