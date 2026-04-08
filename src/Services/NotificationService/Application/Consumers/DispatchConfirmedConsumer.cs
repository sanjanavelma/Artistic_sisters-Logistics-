using Artistic_Sisters.Shared.Events.Payment;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class DispatchConfirmedConsumer : IConsumer<DispatchConfirmedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<DispatchConfirmedConsumer> _logger;

    public DispatchConfirmedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<DispatchConfirmedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DispatchConfirmedEvent> context)
    {
        var evt = context.Message;

        var subject = "Your Order Has Been Dispatched!";
        var body = $@"
            <h2>Great News — Your Order is On The Way!</h2>
            <p>Order ID: <strong>{evt.OrderId}</strong></p>
            <br/>
            <h3>Delivery Agent Details:</h3>
            <p><strong>Agent Name:</strong> {evt.AgentName}</p>
            <p><strong>Contact:</strong> {evt.AgentPhone}</p>
            <p><strong>Vehicle:</strong> {evt.VehicleNumber}</p>
            <br/>
            <p>Dispatched At: {evt.DispatchedAt:dd-MM-yyyy HH:mm}</p>
            <p>Track your order in real-time on our portal.</p>
            <br/>
            <p>— Artistic Sisters Team</p>
        ";

        _logger.LogInformation("Dispatch confirmed email for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(DispatchConfirmedEvent),
            RecipientEmail = "customer@example.com", // Mocked
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
