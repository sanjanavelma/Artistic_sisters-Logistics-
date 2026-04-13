using Artistic_Sisters.Shared.Events.Logistics;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.EmailTemplates;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class AgentAssignedConsumer : IConsumer<AgentAssignedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<AgentAssignedConsumer> _logger;

    public AgentAssignedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<AgentAssignedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AgentAssignedEvent> context)
    {
        var evt = context.Message;
        
        // AgentEmail might be empty if the assigning component didn't provide it
        // We ensure we handle this gracefully
        if (string.IsNullOrWhiteSpace(evt.AgentEmail))
        {
            _logger.LogWarning("Cannot send assignment email to agent {AgentName} for order {OrderId} because AgentEmail is missing.", evt.AgentName, evt.OrderId);
            return;
        }

        var subject = $"New Delivery Assignment — Order {evt.OrderId}";
        var content = $@"
            <h2>You Have a New Delivery Assignment!</h2>
            <p>Hi {evt.AgentName},</p>
            <p>You have been assigned to deliver a new order. Please review the details below:</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> {evt.OrderId}</p>
                <p><strong>Customer Address:</strong> {evt.CustomerAddress}</p>
                <p><strong>SLA Deadline:</strong> {evt.SLADeadline:dd MMM yyyy, HH:mm}</p>
                <p><strong>Assigned Vehicle:</strong> {evt.VehicleNumber}</p>
            </div>
            
            <p>Please log in to your dashboard to view the map and start the delivery process.</p>
            <a href='http://localhost:4200/delivery/dashboard' class='btn'>View Assignment</a>
            <br/><br/>
            <p>Drive safely!</p>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("New Assignment", content);

        await _emailSender.SendEmailAsync(evt.AgentEmail, evt.AgentName, subject, body);

        _logger.LogInformation("Agent assigned email sent to {Email} for order {OrderId}", evt.AgentEmail, evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(AgentAssignedEvent),
            RecipientEmail = evt.AgentEmail,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
