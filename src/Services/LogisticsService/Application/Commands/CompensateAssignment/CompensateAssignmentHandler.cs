// COMPENSATION HANDLER — The Rollback Logic
// This runs when the Saga needs to undo the agent assignment
// Key rule: MUST BE IDEMPOTENT
// If this runs twice, result must be same both times
using LogisticsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogisticsService.Application.Commands.CompensateAssignment;

public class CompensateAssignmentHandler
    : IRequestHandler<CompensateAssignmentCommand, CompensateAssignmentResult>
{
    private readonly LogisticsDbContext _db;
    private readonly ILogger<CompensateAssignmentHandler> _logger;

    public CompensateAssignmentHandler(
        LogisticsDbContext db,
        ILogger<CompensateAssignmentHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CompensateAssignmentResult> Handle(
        CompensateAssignmentCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Compensating assignment for order {OrderId}. Reason: {Reason}",
            request.OrderId, request.Reason);

        // Find the assignment for this order
        // Include Agent and Vehicle so we can release them
        var assignment = await _db.Assignments
            .Include(a => a.Agent)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.OrderId == request.OrderId, ct);

        // ── IDEMPOTENCY CHECK 1 ───────────────────────────────────────────────
        // If no assignment found — maybe it was never created
        // Compensation is already "done" — return success
        if (assignment == null)
        {
            _logger.LogInformation(
                "No assignment found for order {OrderId} — already compensated",
                request.OrderId);
            return new CompensateAssignmentResult
            {
                Success = true,
                Message = "No assignment found — nothing to compensate"
            };
        }

        // ── IDEMPOTENCY CHECK 2 ───────────────────────────────────────────────
        // If already compensated — don't do it again
        // This handles the case where compensation message is delivered twice
        if (assignment.IsCompensated)
        {
            _logger.LogInformation(
                "Assignment for order {OrderId} already compensated",
                request.OrderId);
            return new CompensateAssignmentResult
            {
                Success = true,
                Message = "Already compensated"
            };
        }

        // ── UNDO STEP 1: Mark assignment as cancelled ─────────────────────────
        assignment.Compensate();

        // ── UNDO STEP 2: Release the agent ────────────────────────────────────
        // Agent is now Available again — can take another order
        assignment.Agent.Release();

        // ── UNDO STEP 3: Release the vehicle ─────────────────────────────────
        // Vehicle is available again
        assignment.Vehicle.IsAvailable = true;

        // Save all three changes in ONE transaction
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Compensation complete for order {OrderId}. " +
            "Agent {AgentName} and vehicle {VehicleNumber} released",
            request.OrderId,
            assignment.Agent.Name,
            assignment.Vehicle.RegistrationNumber);

        return new CompensateAssignmentResult
        {
            Success = true,
            Message = "Compensation successful — agent and vehicle released"
        };
    }
}
