// THE HANGFIRE JOB — Runs every 5 minutes automatically
// Checks all active deliveries and fires alerts if SLA is at risk
// This is PROACTIVE — warns BEFORE deadline is missed
using Artistic_Sisters.Shared.Events.Logistics;
using LogisticsService.Domain.Enums;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Jobs;

public class SLAMonitorJob
{
    private readonly LogisticsDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<SLAMonitorJob> _logger;

    public SLAMonitorJob(
        LogisticsDbContext db,
        IPublishEndpoint publisher,
        ILogger<SLAMonitorJob> logger)
    {
        _db = db;
        _publisher = publisher;
        _logger = logger;
    }

    // This method is called by Hangfire every 5 minutes
    // Hangfire passes the method name as a string job ID
    public async Task CheckSLADeadlines()
    {
        _logger.LogInformation(
            "SLA Monitor job started at {Time}", DateTime.UtcNow);

        // Get all ACTIVE deliveries — not delivered, not cancelled
        var activeAssignments = await _db.Assignments
            .Where(a =>
                a.Status != DeliveryStatus.Delivered &&
                a.Status != DeliveryStatus.Cancelled &&
                a.Status != DeliveryStatus.Failed &&
                !a.IsCompensated)
            .ToListAsync();

        int atRiskCount = 0;
        int breachedCount = 0;

        foreach (var assignment in activeAssignments)
        {
            // Calculate % of SLA time remaining
            var remainingPercent = assignment.GetSLARemainingPercent();

            // ── CASE 1: SLA AT RISK — less than 20% time remaining ───────────
            // e.g. 24 hour SLA — alert fires when less than 4.8 hours left
            if (remainingPercent > 0 && remainingPercent < 20)
            {
                var minutesLeft = (int)(assignment.SLADeadline -
                    DateTime.UtcNow).TotalMinutes;

                // Publish SLAAtRisk event
                // Notification Service sends delay warning email to dealer
                await _publisher.Publish(new SLAAtRiskEvent
                {
                    OrderId = assignment.OrderId,
                    SLADeadline = assignment.SLADeadline,
                    MinutesRemaining = minutesLeft,
                    RemainingPercent = remainingPercent
                });

                atRiskCount++;

                _logger.LogWarning(
                    "SLA AT RISK: Order {OrderId} — {Minutes} min left ({Pct:F1}%)",
                    assignment.OrderId, minutesLeft, remainingPercent);
            }

            // ── CASE 2: SLA ALREADY BREACHED ──────────────────────────────────
            if (remainingPercent == 0)
            {
                breachedCount++;
                _logger.LogError(
                    "SLA BREACHED: Order {OrderId} — deadline was {Deadline}",
                    assignment.OrderId, assignment.SLADeadline);
            }
        }

        _logger.LogInformation(
            "SLA Monitor complete. At risk: {AtRisk}, Breached: {Breached}",
            atRiskCount, breachedCount);
    }
}
