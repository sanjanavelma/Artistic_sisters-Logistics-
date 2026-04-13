using System;

namespace Artistic_Sisters.Shared.Events.Order;

/// <summary>
/// Published whenever an order status changes — either by Admin (via OrderService)
/// or by a Delivery Agent (via LogisticsService).
/// 
/// UpdaterRole = "Admin" → Admin made this change; Logistics must respect it as authoritative.
/// UpdaterRole = "Agent" → Agent made this change; OrderService should sync its record.
/// 
/// NotificationService listens to this to email Customer, Agent, and Admin.
/// </summary>
public record OrderStatusChangedEvent
{
    public Guid OrderId { get; init; }

    /// <summary>"Admin" or "Agent"</summary>
    public string UpdaterRole { get; init; } = string.Empty;

    /// <summary>New status as a plain string so all services can consume it without sharing enums.</summary>
    public string NewStatus { get; init; } = string.Empty;

    /// <summary>Previous status — helps Notification Service describe the change clearly.</summary>
    public string OldStatus { get; init; } = string.Empty;

    // ── Customer info ────────────────────────────────────────────────────────
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;

    // ── Agent info (may be empty if no agent assigned yet) ───────────────────
    public string AgentName { get; init; } = string.Empty;
    public string AgentEmail { get; init; } = string.Empty;

    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}
