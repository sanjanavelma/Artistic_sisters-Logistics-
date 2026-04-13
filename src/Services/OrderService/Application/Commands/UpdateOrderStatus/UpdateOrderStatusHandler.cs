using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;
using Artistic_Sisters.Shared.Events.Order;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OrderService.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IConfiguration _config;
    private readonly ILogger<UpdateOrderStatusHandler> _logger;

    public UpdateOrderStatusHandler(
        OrderDbContext db,
        IPublishEndpoint publisher,
        IConfiguration config,
        ILogger<UpdateOrderStatusHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _config = config;
        _logger = logger;
    }

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
        if (order == null)
            return new UpdateOrderStatusResult { Success = false, Message = "Order not found" };

        var oldStatus = order.Status;

        // Admin override — always update the order status
        order.Status = request.Status;

        // ── If status changing to ReadyForDelivery, publish dispatch event ────
        if (oldStatus != OrderStatus.ReadyForDelivery && request.Status == OrderStatus.ReadyForDelivery)
        {
            var outboxMessages = await _db.OutboxMessages
                .Where(m => m.Type == nameof(OrderPlacedEvent))
                .ToListAsync(ct);

            var originalEvent = outboxMessages
                .Select(m =>
                {
                    try { return JsonSerializer.Deserialize<OrderPlacedEvent>(m.Payload); }
                    catch { return null; }
                })
                .FirstOrDefault(e => e != null && e.OrderId == order.Id);

            var dispatchEvent = new ReadyForDispatchEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = originalEvent?.CustomerName ?? "Customer",
                CustomerEmail = originalEvent?.CustomerEmail ?? "",
                CustomerAddress = originalEvent?.ShippingAddress ?? "Local Address",
                ReadyAt = DateTime.UtcNow,
                SLAHours = 24
            };

            await _publisher.Publish(dispatchEvent, ct);
        }

        await _db.SaveChangesAsync(ct);

        // ── Retrieve customer info from the Outbox so we can put it in the event ──
        var outbox = await _db.OutboxMessages
            .Where(m => m.Type == nameof(OrderPlacedEvent))
            .ToListAsync(ct);

        var placedEvent = outbox
            .Select(m =>
            {
                try { return JsonSerializer.Deserialize<OrderPlacedEvent>(m.Payload); }
                catch { return null; }
            })
            .FirstOrDefault(e => e != null && e.OrderId == order.Id);

        // ── Publish OrderStatusChangedEvent so:                              ──
        //   1. LogisticsService syncs the assignment status (Admin has priority)
        //   2. NotificationService emails Customer + Agent + Admin
        var adminEmail = _config["Email:ArtistEmail"] ?? "";
        await _publisher.Publish(new OrderStatusChangedEvent
        {
            OrderId = order.Id,
            UpdaterRole = "Admin",
            NewStatus = request.Status.ToString(),
            OldStatus = oldStatus.ToString(),
            CustomerName = placedEvent?.CustomerName ?? "Customer",
            CustomerEmail = placedEvent?.CustomerEmail ?? "",
            // Agent info not known here — LogisticsService will already have it.
            // The consumer in LogisticsService will handle agent-side sync.
            AgentName = "",
            AgentEmail = "",
            ChangedAt = DateTime.UtcNow
        }, ct);

        _logger.LogInformation(
            "Admin updated Order {OrderId} from {Old} to {New}",
            order.Id, oldStatus, request.Status);

        return new UpdateOrderStatusResult
        {
            Success = true,
            Message = $"Order status updated to {request.Status}"
        };
    }
}
