using Artistic_Sisters.Shared.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.EmailTemplates;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class DeliveryAgentRegisteredConsumer : IConsumer<DeliveryAgentRegisteredEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<DeliveryAgentRegisteredConsumer> _logger;

    public DeliveryAgentRegisteredConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<DeliveryAgentRegisteredConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeliveryAgentRegisteredEvent> context)
    {
        var evt = context.Message;

        var subject = "Welcome to Artistic Sisters Logistics Team!";
        var content = $@"
            <h2>Welcome aboard, {evt.Name}!</h2>
            <p>You have been successfully registered as a Delivery Agent.</p>
            <div class='info-box'>
                <p><strong>Your Role:</strong> Delivery Agent</p>
                <p><strong>Phone Registered:</strong> {evt.Phone}</p>
                <p><strong>Registered At:</strong> {evt.RegisteredAt:dd MMM yyyy, HH:mm}</p>
            </div>
            
            <p>You can now log into the delivery portal to view orders assigned to you, navigate to customer addresses, and mark deliveries as complete.</p>
            <a href='http://localhost:4200/login' class='btn'>Go to Delivery Portal</a>
            <br/><br/>
            <p>We are thrilled to have you ensuring our art reaches customers safely.</p>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("Delivery Logistics Team", content);

        await _emailSender.SendEmailAsync(evt.Email, evt.Name, subject, body);

        _logger.LogInformation("Delivery agent welcome email sent for agent {AgentId} to {Email}", evt.AgentId, evt.Email);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(DeliveryAgentRegisteredEvent),
            RecipientEmail = evt.Email,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
