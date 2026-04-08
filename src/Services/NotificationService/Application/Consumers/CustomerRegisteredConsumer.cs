using Artistic_Sisters.Shared.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

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
        var body = $@"
            <h2>Welcome to Artistic Sisters, {evt.Name}!</h2>
            <p>Your registration has been received successfully.</p>
            <p>Your account is currently <strong>pending admin approval</strong>.</p>
            <p>You will receive another email once your account is approved.</p>
            <br/>
            <p><strong>Registered At:</strong> {evt.RegisteredAt:dd-MM-yyyy HH:mm}</p>
            <br/>
            <p>Thank you for registering!</p>
            <p>— Artistic Sisters Team</p>
        ";

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
