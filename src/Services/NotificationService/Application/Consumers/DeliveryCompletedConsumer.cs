using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly NotificationDbContext _db;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    public DeliveryCompletedConsumer(
        NotificationDbContext db,
        ILogger<DeliveryCompletedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation("Delivery completed for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(DeliveryCompletedEvent),
            RecipientEmail = "customer@example.com", // Mocked
            Subject = $"Order Delivered Successfully — {evt.OrderId}",
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
