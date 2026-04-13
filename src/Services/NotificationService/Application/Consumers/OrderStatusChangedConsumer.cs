using Artistic_Sisters.Shared.Events.Order;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.EmailTemplates;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Consumers;

/// <summary>
/// Fires on every OrderStatusChangedEvent — whether the change was made by
/// an Admin or a Delivery Agent.
/// 
/// Sends targeted emails to:
///   1. Customer   — "Your order status has been updated"
///   2. Agent      — "Order you are delivering has been updated" (skipped if no agent email)
///   3. Admin      — "Status change alert" (uses Email:ArtistEmail from config)
/// 
/// NOTE: We skip "Delivered" customer emails here because DeliveryCompletedConsumer
/// already sends a richer delivered email to the customer. We still notify Admin + Agent.
/// </summary>
public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(
        IEmailSender emailSender,
        NotificationDbContext db,
        IConfiguration config,
        ILogger<OrderStatusChangedConsumer> logger)
    {
        _emailSender = emailSender;
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var evt = context.Message;
        var updaterLabel = evt.UpdaterRole == "Admin" ? "Admin" : "Delivery Agent";
        var shortOrderId = evt.OrderId.ToString().Substring(0, 8).ToUpper();

        // ── 1. Email Customer (skip if Delivered — DeliveryCompletedConsumer covers it) ──
        if (!string.Equals(evt.NewStatus, "Delivered", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(evt.CustomerEmail))
        {
            var customerSubject = $"Order #{shortOrderId} Status Update — {evt.NewStatus}";
            var customerContent = $@"
                <h2>Your Order Status Has Been Updated</h2>
                <p>Dear {evt.CustomerName},</p>
                <p>Your order status has changed.</p>
                <div class='info-box'>
                    <p><strong>Order ID:</strong> {evt.OrderId}</p>
                    <p><strong>Previous Status:</strong> {FriendlyStatus(evt.OldStatus)}</p>
                    <p><strong>New Status:</strong> {FriendlyStatus(evt.NewStatus)}</p>
                    <p><strong>Updated At:</strong> {evt.ChangedAt:dd MMM yyyy, HH:mm} UTC</p>
                    <p><strong>Updated By:</strong> {updaterLabel}</p>
                </div>
                <p>You can track your order on your dashboard.</p>
                <a href='http://localhost:4200/customer/dashboard' class='btn'>Track My Order</a>
                <br/><br/>
                <p>— The Artistic Sisters Team</p>
            ";
            var customerBody = EmailTemplateBuilder.Build("Order Status Update", customerContent);
            await SendAndLog(evt.CustomerEmail, evt.CustomerName, customerSubject, customerBody, nameof(OrderStatusChangedEvent) + "_Customer");
            _logger.LogInformation("Status update email sent to customer {Email} for order {OrderId}", evt.CustomerEmail, evt.OrderId);
        }

        // ── 2. Email Delivery Agent (if we have their email) ──────────────────
        if (!string.IsNullOrWhiteSpace(evt.AgentEmail))
        {
            var agentSubject = $"Order #{shortOrderId} Status Update — {evt.NewStatus}";
            var agentContent = $@"
                <h2>Order Status Changed</h2>
                <p>Hi {evt.AgentName},</p>
                <p>The status of an order assigned to you has been updated{(evt.UpdaterRole == "Admin" ? " by the <strong>Admin</strong>" : "")}.</p>
                <div class='info-box'>
                    <p><strong>Order ID:</strong> {evt.OrderId}</p>
                    <p><strong>Previous Status:</strong> {FriendlyStatus(evt.OldStatus)}</p>
                    <p><strong>New Status:</strong> {FriendlyStatus(evt.NewStatus)}</p>
                    <p><strong>Updated At:</strong> {evt.ChangedAt:dd MMM yyyy, HH:mm} UTC</p>
                </div>
                {(evt.UpdaterRole == "Admin" && string.Equals(evt.NewStatus, "Delivered", StringComparison.OrdinalIgnoreCase)
                    ? "<p><strong>The Admin has marked this order as delivered. You are now released from this assignment.</strong></p>"
                    : "<p>Please update the customer accordingly and continue with your delivery process.</p>")}
                <a href='http://localhost:4200/delivery/dashboard' class='btn'>View Dashboard</a>
                <br/><br/>
                <p>— The Artistic Sisters Team</p>
            ";
            var agentBody = EmailTemplateBuilder.Build("Delivery Update", agentContent);
            await SendAndLog(evt.AgentEmail, evt.AgentName, agentSubject, agentBody, nameof(OrderStatusChangedEvent) + "_Agent");
            _logger.LogInformation("Status update email sent to agent {Email} for order {OrderId}", evt.AgentEmail, evt.OrderId);
        }

        // ── 3. Email Admin (always — so admin knows what Agent did, and confirms their own) ──
        var adminEmail = _config["Email:ArtistEmail"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var adminSubject = $"[Admin] Order #{shortOrderId} — Status changed to {evt.NewStatus}";
            var adminContent = $@"
                <h2>Order Status Change Alert</h2>
                <p>Hi Sanjana,</p>
                <p>An order status has changed in the system.</p>
                <div class='info-box'>
                    <p><strong>Order ID:</strong> {evt.OrderId}</p>
                    <p><strong>Changed By:</strong> {updaterLabel}</p>
                    <p><strong>Previous Status:</strong> {FriendlyStatus(evt.OldStatus)}</p>
                    <p><strong>New Status:</strong> {FriendlyStatus(evt.NewStatus)}</p>
                    <p><strong>Customer:</strong> {evt.CustomerName} ({evt.CustomerEmail})</p>
                    {(!string.IsNullOrWhiteSpace(evt.AgentName) ? $"<p><strong>Agent:</strong> {evt.AgentName} ({evt.AgentEmail})</p>" : "")}
                    <p><strong>Updated At:</strong> {evt.ChangedAt:dd MMM yyyy, HH:mm} UTC</p>
                </div>
                <a href='http://localhost:4200/admin/dashboard' class='btn'>Go to Admin Dashboard</a>
                <br/><br/>
                <p>— Artistic Sisters System</p>
            ";
            var adminBody = EmailTemplateBuilder.Build("Order Status Alert", adminContent);
            await SendAndLog(adminEmail, "Sanjana", adminSubject, adminBody, nameof(OrderStatusChangedEvent) + "_Admin");
            _logger.LogInformation("Status update admin alert sent for order {OrderId}", evt.OrderId);
        }

        await _db.SaveChangesAsync();
    }

    private async Task SendAndLog(string email, string name, string subject, string body, string eventType)
    {
        await _emailSender.SendEmailAsync(email, name, subject, body);
        _db.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            RecipientEmail = email,
            Subject = subject,
            IsSuccess = true,
            SentAt = DateTime.UtcNow
        });
    }

    /// <summary>Converts internal enum string to a customer-friendly label.</summary>
    private static string FriendlyStatus(string status) => status switch
    {
        "Pending"          => "Pending",
        "Confirmed"        => "Confirmed",
        "InProduction"     => "In Production",
        "ReadyForDelivery" => "Ready for Dispatch",
        "OutForDelivery"   => "Out for Delivery",
        "Delivered"        => "Delivered ✅",
        "Cancelled"        => "Cancelled",
        // Fallbacks for Logistics strings
        "PickedUp"         => "Picked Up",
        "InTransit"        => "In Transit",
        "Failed"           => "Delivery Failed",
        _                  => status
    };
}
