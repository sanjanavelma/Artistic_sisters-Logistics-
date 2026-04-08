using LogisticsService.Application.Commands.AssignAgent;
using LogisticsService.Application.Commands.CompensateAssignment;
using LogisticsService.Application.Commands.UpdateDeliveryStatus;
using LogisticsService.Infrastructure.Cache;
using LogisticsService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisticsService.API.Controllers;

[ApiController]
[Route("api/logistics")]
public class LogisticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LogisticsDbContext _db;
    private readonly ILogisticsCache _cache;

    public LogisticsController(
        IMediator mediator,
        LogisticsDbContext db,
        ILogisticsCache cache)
    {
        _mediator = mediator;
        _db = db;
        _cache = cache;
    }

    // POST api/logistics/assign — assign agent to order
    [HttpPost("assign")]
    public async Task<IActionResult> AssignAgent(
        [FromBody] AssignAgentCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    // POST api/logistics/compensate — rollback assignment
    [HttpPost("compensate")]
    public async Task<IActionResult> Compensate(
        [FromBody] CompensateAssignmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // PUT api/logistics/status — agent updates delivery status
    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(
        [FromBody] UpdateDeliveryStatusCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // GET api/logistics/track/{orderId} — get live GPS from Redis
    [HttpGet("track/{orderId}")]
    public async Task<IActionResult> TrackOrder(Guid orderId)
    {
        // Read GPS from Redis — instant response
        var gps = await _cache.GetGPSAsync(orderId);
        if (gps == null)
            return NotFound(new { message = "No tracking data available" });

        return Ok(new
        {
            OrderId = orderId,
            Latitude = gps.Value.Lat,
            Longitude = gps.Value.Lng,
            UpdatedAt = DateTime.UtcNow
        });
    }

    // GET api/logistics/agents — get all agents and their status
    [HttpGet("agents")]
    public async Task<IActionResult> GetAgents()
    {
        var agents = await _db.Agents
            .Where(a => a.IsActive)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Phone,
                Status = a.Status.ToString(),
                a.CurrentOrderId
            })
            .ToListAsync();

        return Ok(agents);
    }

    // POST api/logistics/agents — add new agent (for testing/seeding)
    [HttpPost("agents")]
    public async Task<IActionResult> AddAgent(
        [FromBody] AddAgentRequest request)
    {
        var agent = new Domain.Entities.DeliveryAgent
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Status = Domain.Enums.AgentStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Agent added", agentId = agent.Id });
    }

    // GET api/logistics/assignments — all assignments (for admin dashboard)
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAllAssignments()
    {
        var assignments = await _db.Assignments
            .Include(a => a.Agent)
            .Include(a => a.Vehicle)
            .Select(a => new
            {
                a.Id,
                a.OrderId,
                a.Status,
                StatusText = a.Status.ToString(),
                a.AssignedAt,
                a.SLADeadline,
                a.IsCompensated,
                a.LastLatitude,
                a.LastLongitude,
                a.LastGPSUpdate,
                Agent = new { a.Agent.Id, a.Agent.Name, a.Agent.Phone, a.Agent.Email },
                Vehicle = new { a.Vehicle.RegistrationNumber, a.Vehicle.VehicleType }
            })
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        return Ok(assignments);
    }

    // GET api/logistics/assignments/agent/{agentId} — assignments for a specific delivery agent
    [HttpGet("assignments/agent/{agentId}")]
    public async Task<IActionResult> GetAgentAssignments(Guid agentId)
    {
        var assignments = await _db.Assignments
            .Include(a => a.Agent)
            .Include(a => a.Vehicle)
            .Where(a => a.AgentId == agentId && !a.IsCompensated)
            .Select(a => new
            {
                a.Id,
                a.OrderId,
                a.Status,
                StatusText = a.Status.ToString(),
                a.AssignedAt,
                a.SLADeadline,
                a.LastLatitude,
                a.LastLongitude,
                a.LastGPSUpdate,
                Agent = new { a.Agent.Id, a.Agent.Name, a.Agent.Phone, a.Agent.Email },
                Vehicle = new { a.Vehicle.RegistrationNumber, a.Vehicle.VehicleType }
            })
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        return Ok(assignments);
    }

    // POST api/logistics/gps — agent pushes GPS update to Redis only (fast, no DB write)
    [HttpPost("gps")]
    public async Task<IActionResult> UpdateGPS([FromBody] UpdateGPSRequest request)
    {
        await _cache.SetGPSAsync(request.OrderId, request.Latitude, request.Longitude);
        return Ok(new { message = "GPS updated" });
    }

    // POST api/logistics/vehicles — add new vehicle (for testing/seeding)
    [HttpPost("vehicles")]
    public async Task<IActionResult> AddVehicle(
        [FromBody] AddVehicleRequest request)
    {
        var vehicle = new Domain.Entities.Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = request.RegistrationNumber,
            VehicleType = request.VehicleType,
            IsAvailable = true,
            IsActive = true
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Vehicle added",
            vehicleId = vehicle.Id
        });
    }
}

// Simple request models for adding agents and vehicles
public record AddAgentRequest
{
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record AddVehicleRequest
{
    public string RegistrationNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
}

public record UpdateGPSRequest
{
    public Guid OrderId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

