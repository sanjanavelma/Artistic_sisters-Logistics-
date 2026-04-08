using Artistic_Sisters.Shared.Events.Logistics;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class SLAAtRiskConsumer : IConsumer<SLAAtRiskEvent>
{
    private readonly NotificationDbContext _db;
    private readonly ILogger<SLAAtRiskConsumer> _logger;

    public SLAAtRiskConsumer(
        NotificationDbContext db,
        ILogger<SLAAtRiskConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SLAAtRiskEvent> context)
    {
        var evt = context.Message;

        _logger.LogWarning("SLA at risk for order {OrderId} — {Minutes} minutes remaining", evt.OrderId, evt.MinutesRemaining);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(SLAAtRiskEvent),
            RecipientEmail = "customer@example.com", // Mocked
            Subject = $"Delivery Delay Alert — Order {evt.OrderId}",
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation("SLA alert logged for order {OrderId}", evt.OrderId);
    }
}
