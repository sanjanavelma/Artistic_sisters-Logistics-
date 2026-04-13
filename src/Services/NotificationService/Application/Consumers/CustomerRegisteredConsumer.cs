using Artistic_Sisters.Shared.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Application.EmailTemplates;

namespace NotificationService.Application.Consumers;

public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<CustomerRegisteredConsumer> _logger;

    public CustomerRegisteredConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<CustomerRegisteredConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation("Sending registration email to {Email}", evt.Email);

        var subject = "Registration Received — Artistic Sisters";
        var content = $@"
            <h2>Welcome to Artistic Sisters, {evt.Name}!</h2>
            <p>Your registration has been received successfully.</p>
            <div class='info-box'>
                <p>Welcome to the community! You can now log into your account and start exploring our art collections.</p>
            </div>
            <p><strong>Registered At:</strong> {evt.RegisteredAt:dd-MM-yyyy HH:mm}</p>
            <br/>
            <p>Thank you for registering!</p>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("Welcome to Artistic Sisters", content);

        await _emailSender.SendEmailAsync(evt.Email, evt.Name, subject, body);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(CustomerRegisteredEvent),
            RecipientEmail = evt.Email,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
