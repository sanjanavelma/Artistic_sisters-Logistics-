using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Application.EmailTemplates;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace NotificationService.Application.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        IConfiguration config,
        ILogger<OrderPlacedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var evt = context.Message;

        // ── 1. Create Invoice for Customer ────────────────────────────────────
        var subject = $"Order Confirmed — #{evt.OrderId.ToString().Substring(0, 8)}";
        
        var invoiceHtml = new StringBuilder();
        invoiceHtml.Append($@"
            <h2>Thank You For Your Order!</h2>
            <p>Dear {evt.CustomerName},</p>
            <p>Your order has been placed successfully and is now being processed.</p>
            <div class='info-box'>
                <p><strong>Order ID:</strong> {evt.OrderId}</p>
                <p><strong>Date:</strong> {evt.PlacedAt:dd MMM yyyy, HH:mm}</p>
                <p><strong>Payment Mode:</strong> {evt.PaymentMode}</p>
                <p><strong>Shipping Address:</strong> {evt.ShippingAddress}</p>
            </div>
            
            <h3>Order Summary</h3>
            <table class='item-table'>
                <tr>
                    <th>Artwork</th>
                    <th>Qty</th>
                    <th>Price</th>
                    <th>Subtotal</th>
                </tr>
        ");

        foreach (var item in evt.Items)
        {
            var subtotal = item.Quantity * item.UnitPrice;
            invoiceHtml.Append($@"
                <tr>
                    <td>{item.ArtworkTitle}</td>
                    <td>{item.Quantity}</td>
                    <td>Rs. {item.UnitPrice:F2}</td>
                    <td>Rs. {subtotal:F2}</td>
                </tr>
            ");
        }

        invoiceHtml.Append($@"
            </table>
            <div class='grand-total'>
                Total Amount: Rs. {evt.TotalAmount:F2}
            </div>
            
            <p>You will receive another email once your order has been dispatched.</p>
        ");

        var customerBody = EmailTemplateBuilder.Build("Order Confirmation", invoiceHtml.ToString());
        
        // Use the email provided in the event
        var customerEmail = evt.CustomerEmail;
        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            await _emailSender.SendEmailAsync(customerEmail, evt.CustomerName, subject, customerBody);

            _logger.LogInformation("Order placement invoice sent to customer {Email} for order {OrderId}", customerEmail, evt.OrderId);

            _db.NotificationLogs.Add(new NotificationLog
            {
                Id = Guid.NewGuid(),
                EventType = nameof(OrderPlacedEvent),
                RecipientEmail = customerEmail,
                Subject = subject,
                IsSuccess = true,
                SentAt = DateTime.UtcNow
            });
        }
        else
        {
            _logger.LogWarning("Cannot send Order Confirmation: CustomerEmail is missing for Order {OrderId}", evt.OrderId);
        }

        // ── 2. Alert the Artist ───────────────────────────────────────────────
        var artistEmail = _config["Email:ArtistEmail"];
        if (!string.IsNullOrWhiteSpace(artistEmail))
        {
            var artistSubject = $"New Order Received! — {evt.TotalAmount:C}";
            var artistBody = EmailTemplateBuilder.Build("New Order Alert", $@"
                <h2>You have a new order!</h2>
                <p>A new order was just placed by <strong>{evt.CustomerName}</strong>.</p>
                <div class='info-box'>
                    <p><strong>Order ID:</strong> {evt.OrderId}</p>
                    <p><strong>Total Value:</strong> Rs. {evt.TotalAmount:F2}</p>
                </div>
                <p>Please log in to the admin dashboard to review and prepare this order for dispatch.</p>
                <a href='http://localhost:4200/admin' class='btn'>Go to Dashboard</a>
            ");
            
            await _emailSender.SendEmailAsync(artistEmail, "Artistic Sisters", artistSubject, artistBody);
        }

        await _db.SaveChangesAsync();
    }
}
