using Artistic_Sisters.Shared.Events.Identity;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Application.EmailTemplates;

namespace NotificationService.Application.Consumers;

public class CustomerApprovedConsumer : IConsumer<CustomerApprovedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly ILogger<CustomerApprovedConsumer> _logger;

    public CustomerApprovedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        ILogger<CustomerApprovedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerApprovedEvent> context)
    {
        var evt = context.Message;

        var subject = "Account Approved — Welcome to Artistic Sisters!";
        var content = $@"
            <h2>Congratulations, {evt.Name}!</h2>
            <p>Your account has been <strong>approved</strong>.</p>
            <p>You can now log in, browse our exclusive artwork collections, and start placing orders.</p>
            <a href='http://localhost:4200/login' class='btn'>Login Now</a>
            <br/><br/>
            <p><strong>Approved At:</strong> {evt.ApprovedAt:dd-MM-yyyy HH:mm}</p>
            <br/>
            <p>— The Artistic Sisters Team</p>
        ";

        var body = EmailTemplateBuilder.Build("Account Approved", content);

        await _emailSender.SendEmailAsync(evt.Email, evt.Name, subject, body);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(CustomerApprovedEvent),
            RecipientEmail = evt.Email,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
