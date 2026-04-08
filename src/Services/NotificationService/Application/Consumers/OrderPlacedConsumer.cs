using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<OrderPlacedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var evt = context.Message;

        var subject = $"Order Confirmed — {evt.OrderId}";
        var body = $@"
            <h2>Your Order Has Been Placed!</h2>
            <p>Order ID: <strong>{evt.OrderId}</strong></p>
            <p>Type: <strong>{evt.OrderType}</strong></p>
            <p>Total Amount: <strong>Rs. {evt.TotalAmount:F2}</strong></p>
            <p>Placed At: {evt.PlacedAt:dd-MM-yyyy HH:mm}</p>
            <br/>
            <p>Your order is currently being processed.</p>
            <p>You will receive updates as your order progresses.</p>
            <br/>
            <p>— Artistic Sisters Team</p>
        ";

        _logger.LogInformation("Order placed email for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(OrderPlacedEvent),
            RecipientEmail = "customer@example.com", // Mocked for now
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
