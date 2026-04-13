// Listens for OrderPlacedEvent from Order Service
// Creates a payment record for the new order
using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly PaymentDbContext _db;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(PaymentDbContext db,
        ILogger<OrderPlacedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Order placed event received for order {OrderId}", evt.OrderId);

        // Idempotency — don't create duplicate payment records
        var exists = await _db.Payments
            .AnyAsync(p => p.OrderId == evt.OrderId);
        if (exists) return;

        // Create payment record
        var payment = new PaymentRecord
        {
            Id = Guid.NewGuid(),
            OrderId = evt.OrderId,
            CustomerId = evt.CustomerId,
            Amount = evt.TotalAmount,
            Status = PaymentStatus.Pending,
            PaymentMode = evt.PaymentMode,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Payment record created for order {OrderId}, amount {Amount}",
            evt.OrderId, evt.TotalAmount);
    }
}
