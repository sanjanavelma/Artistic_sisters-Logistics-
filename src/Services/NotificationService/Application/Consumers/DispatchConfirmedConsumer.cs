using Artistic_Sisters.Shared.Events.Payment;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Application.EmailTemplates;

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
        var content = $@"
            <h2>Great News — Your Order is On The Way!</h2>
            <p>Dear {evt.CustomerName},</p>
            <p>Your order is out for delivery. You can track it using the details below.</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> {evt.OrderId}</p>
                <p><strong>Dispatched At:</strong> {evt.DispatchedAt:dd MMM yyyy, HH:mm}</p>
            </div>
            
            <h3>Delivery Agent Details</h3>
            <table class='item-table'>
                <tr>
                    <td><strong>Agent Name:</strong></td>
                    <td>{evt.AgentName}</td>
                </tr>
                <tr>
                    <td><strong>Contact:</strong></td>
                    <td>{evt.AgentPhone}</td>
                </tr>
                <tr>
                    <td><strong>Vehicle:</strong></td>
                    <td>{evt.VehicleNumber}</td>
                </tr>
            </table>
            
            <a href='http://localhost:4200/track/{evt.OrderId}' class='btn'>Track Delivery</a>
            <br/><br/>
            <p>Have a great day!</p>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("Order Dispatched", content);
        
        var customerEmail = evt.CustomerEmail;
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            _logger.LogWarning("Cannot send Dispatch Notification: CustomerEmail is missing for Order {OrderId}", evt.OrderId);
            return;
        }
        await _emailSender.SendEmailAsync(customerEmail, evt.CustomerName, subject, body);

        _logger.LogInformation("Dispatch confirmed email for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(DispatchConfirmedEvent),
            RecipientEmail = customerEmail,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
