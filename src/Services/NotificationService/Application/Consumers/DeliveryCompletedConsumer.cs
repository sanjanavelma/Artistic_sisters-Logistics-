using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Email;
using NotificationService.Application.EmailTemplates;

namespace NotificationService.Application.Consumers;

public class DeliveryCompletedConsumer : IConsumer<DeliveryCompletedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<DeliveryCompletedConsumer> _logger;

    public DeliveryCompletedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<DeliveryCompletedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryCompletedEvent> context)
    {
        var evt = context.Message;

        var subject = $"Order Delivered Successfully — {evt.OrderId}";
        var content = $@"
            <h2>Your Delivery is Complete!</h2>
            <p>Dear {evt.CustomerName},</p>
            <p>Your order has been successfully delivered and handed over to you.</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> {evt.OrderId}</p>
                <p><strong>Delivered At:</strong> {evt.DeliveredAt:dd MMM yyyy, HH:mm}</p>
            </div>
            <p>We hope you enjoy your new artwork! Please share your thoughts with us by leaving a review on our portal.</p>
            <a href='http://localhost:4200/customer/dashboard' class='btn'>Go to Dashboard</a>
            <br/><br/>
            <p>Thank you for choosing Artistic Sisters.</p>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("Delivery Complete", content);
        
        var customerEmail = evt.CustomerEmail;
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            _logger.LogWarning("Cannot send Delivery Completion: CustomerEmail is missing for Order {OrderId}", evt.OrderId);
            return;
        }
        await _emailSender.SendEmailAsync(customerEmail, evt.CustomerName, subject, body);

        _logger.LogInformation("Delivery completed for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(DeliveryCompletedEvent),
            RecipientEmail = customerEmail,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
