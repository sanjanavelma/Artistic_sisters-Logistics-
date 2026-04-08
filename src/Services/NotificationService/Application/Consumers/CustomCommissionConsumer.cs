using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

public class CustomCommissionConsumer : IConsumer<CustomCommissionPlacedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<CustomCommissionConsumer> _logger;

    public CustomCommissionConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        IConfiguration config,
        ILogger<CustomCommissionConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomCommissionPlacedEvent> context)
    {
        var evt = context.Message;

        // ── Email to YOU (the artist) ─────────────────────────────────────
        var artistSubject = $"New Commission Request from {evt.CustomerName}!";
        var artistBody = $@"
            <h2>New Custom Commission Request!</h2>
            <p><strong>Customer:</strong> {evt.CustomerName}</p>
            <p><strong>Email:</strong> {evt.CustomerEmail}</p>
            <hr/>
            <h3>Commission Details:</h3>
            <p><strong>Artwork Type:</strong> {evt.ArtworkType}</p>
            <p><strong>Medium:</strong> {evt.Medium}</p>
            <p><strong>Size:</strong> {evt.Size}</p>
            <p><strong>Budget:</strong> Rs.{evt.BudgetMin} - Rs.{evt.BudgetMax}</p>
            <p><strong>Special Instructions:</strong> {evt.SpecialInstructions}</p>
            <p><strong>Reference Photo:</strong> <a href='{evt.ReferencePhotoUrl}'>View Photo</a></p>
            <hr/>
            <p>Order ID: {evt.OrderId}</p>
            <p>Placed At: {evt.PlacedAt:dd-MM-yyyy HH:mm}</p>
            <br/>
            <p>Login to your admin panel to review and quote.</p>
        ";

        // Send email to artist
        await _emailSender.SendEmailAsync(
            _config["Email:ArtistEmail"]!,
            "Artistic Sisters",
            artistSubject,
            artistBody);

        // ── Confirmation email to customer ────────────────────────────────
        var customerSubject = "Commission Request Received!";
        var customerBody = $@"
            <h2>Thank you, {evt.CustomerName}!</h2>
            <p>Your custom commission request has been received.</p>
            <h3>Your Request:</h3>
            <p><strong>Type:</strong> {evt.ArtworkType}</p>
            <p><strong>Medium:</strong> {evt.Medium}</p>
            <p><strong>Size:</strong> {evt.Size}</p>
            <p><strong>Budget:</strong> Rs.{evt.BudgetMin} - Rs.{evt.BudgetMax}</p>
            <br/>
            <p>We will review your request and get back to you within 24 hours with a price quote.</p>
            <br/>
            <p>— Artistic Sisters Team</p>
        ";

        await _emailSender.SendEmailAsync(
            evt.CustomerEmail,
            evt.CustomerName,
            customerSubject,
            customerBody);

        _logger.LogInformation("Commission emails sent for order {OrderId}", evt.OrderId);

        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = nameof(CustomCommissionPlacedEvent),
            RecipientEmail = evt.CustomerEmail,
            Subject = customerSubject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}
