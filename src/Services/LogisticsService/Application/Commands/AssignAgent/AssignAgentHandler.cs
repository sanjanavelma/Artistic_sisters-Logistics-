// THE MOST IMPORTANT HANDLER in Logistics Service
// This is the Saga participant — it does the agent assignment step
// On success → publishes AgentAssignedEvent → Saga continues
// On failure → publishes AssignAgentFailedEvent → Saga starts compensation
using Artistic_Sisters.Shared.Events.Logistics;
using LogisticsService.Domain.Entities;
using LogisticsService.Infrastructure.Cache;
using LogisticsService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Commands.AssignAgent;

public class AssignAgentHandler
    : IRequestHandler<AssignAgentCommand, AssignAgentResult>
{
    private readonly LogisticsDbContext _db;
    private readonly ILogisticsCache _cache;
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<AssignAgentHandler> _logger;

    public AssignAgentHandler(
        LogisticsDbContext db,
        ILogisticsCache cache,
        IPublishEndpoint publisher,
        ILogger<AssignAgentHandler> logger)
    {
        _db = db;
        _cache = cache;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<AssignAgentResult> Handle(
        AssignAgentCommand request, CancellationToken ct)
    {
        // ── DEFENSIVE CHECK: Prevent duplicate assignment for the same order ──
        var existingAssignment = await _db.Assignments
            .FirstOrDefaultAsync(a => a.OrderId == request.OrderId, ct);
            
        if (existingAssignment != null)
        {
            return new AssignAgentResult
            {
                Success = false,
                FailureReason = $"Order {request.OrderId} has already been assigned and is in status: {existingAssignment.Status}"
            };
        }

        // ── STEP 1: Find available agent ──────────────────────────────────────
        DeliveryAgent? agent = null;

        // If logistics manager specified a preferred agent — try that first
        if (request.PreferredAgentId.HasValue)
        {
            agent = await _db.Agents
                .FirstOrDefaultAsync(a =>
                    a.Id == request.PreferredAgentId &&
                    a.IsActive, ct);

            // Check if preferred agent is actually available
            if (agent != null && !agent.IsAvailable())
            {
                // Preferred agent is busy — fall back to any available
                agent = null;
            }
        }

        // If no preferred agent or preferred was busy — find any available
        if (agent == null)
        {
            agent = await _db.Agents
                .Where(a => a.IsActive &&
                       a.Status == Domain.Enums.AgentStatus.Available)
                .FirstOrDefaultAsync(ct);
        }

        // ── STEP 2: No agent available — TRIGGER SAGA FAILURE ────────────────
        if (agent == null)
        {
            _logger.LogWarning(
                "No available agent for order {OrderId}", request.OrderId);

            // Publish failure event — Saga Orchestrator will start compensation
            // This rolls back stock deduction and payment lock
            await _publisher.Publish(new AssignAgentFailedEvent
            {
                OrderId = request.OrderId,
                Reason = "No available delivery agent at this time",
                FailedAt = DateTime.UtcNow
            });

            return new AssignAgentResult
            {
                Success = false,
                FailureReason = "No available delivery agent"
            };
        }

        // ── STEP 3: Find available vehicle ────────────────────────────────────
        var vehicle = await _db.Vehicles
            .Where(v => v.IsAvailable && v.IsActive)
            .FirstOrDefaultAsync(ct);

        // No vehicle available — also a failure
        if (vehicle == null)
        {
            _logger.LogWarning(
                "No available vehicle for order {OrderId}", request.OrderId);

            await _publisher.Publish(new AssignAgentFailedEvent
            {
                OrderId = request.OrderId,
                Reason = "No available vehicle at this time",
                FailedAt = DateTime.UtcNow
            });

            return new AssignAgentResult
            {
                Success = false,
                FailureReason = "No available vehicle"
            };
        }

        // ── STEP 4: Create the assignment ─────────────────────────────────────
        // Use factory method from entity
        var assignment = DeliveryAssignment.Create(
            request.OrderId,
            agent.Id,
            vehicle.Id,
            request.SLAHours,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerAddress);

        // Mark agent as busy — they can't take another order
        agent.AssignToOrder(request.OrderId);

        // Mark vehicle as unavailable
        vehicle.IsAvailable = false;

        // Save to LogisticsDB — LOCAL transaction only
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync(ct);

        // ── STEP 5: Initialize GPS in Redis ───────────────────────────────────
        // Start with 0,0 — agent will update as they move
        await _cache.SetGPSAsync(request.OrderId, 0, 0);

        // ── STEP 6: Publish SUCCESS event ─────────────────────────────────────
        // Saga Orchestrator receives this and confirms dispatch
        // Notification Service sends customer tracking info + agent assignment email
        await _publisher.Publish(new AgentAssignedEvent
        {
            OrderId = request.OrderId,
            AssignmentId = assignment.Id,
            AgentName = agent.Name,
            AgentEmail = agent.Email,
            AgentPhone = agent.Phone,
            VehicleNumber = vehicle.RegistrationNumber,
            CustomerAddress = request.CustomerAddress,
            SLADeadline = assignment.SLADeadline
        });

        _logger.LogInformation(
            "Agent {AgentName} assigned to order {OrderId}",
            agent.Name, request.OrderId);

        return new AssignAgentResult
        {
            Success = true,
            Message = $"Agent {agent.Name} assigned successfully",
            AssignmentId = assignment.Id,
            AgentName = agent.Name,
            AgentPhone = agent.Phone,
            VehicleNumber = vehicle.RegistrationNumber,
            SLADeadline = assignment.SLADeadline
        };
    }
}
