using Artistic_Sisters.Shared.Events.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Application.Commands.RegisterDeliveryAgent;

public class RegisterDeliveryAgentHandler
    : IRequestHandler<RegisterDeliveryAgentCommand, RegisterDeliveryAgentResult>
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IConfiguration _config;

    public RegisterDeliveryAgentHandler(
        IdentityDbContext db, IPublishEndpoint publisher, IConfiguration config)
    {
        _db = db; _publisher = publisher; _config = config;
    }

    public async Task<RegisterDeliveryAgentResult> Handle(
        RegisterDeliveryAgentCommand request, CancellationToken ct)
    {
        // Check email not already taken
        var exists = await _db.Customers
            .AnyAsync(c => c.Email == request.Email, ct);
        if (exists)
            return new RegisterDeliveryAgentResult
            { Success = false, Message = "Email already registered" };

        // Create user record with DeliveryAgent role
        var agent = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Address = request.Address,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow,
            Role = "DeliveryAgent"  // FORCE ROLE TO DELIVERY AGENT
        };

        _db.Customers.Add(agent);
        await _db.SaveChangesAsync(ct);

        // Publish event so Notification service can send welcome email
        await _publisher.Publish(new CustomerRegisteredEvent
        {
            CustomerId = agent.Id,
            Name = agent.Name,
            Email = agent.Email,
            RegisteredAt = agent.RegisteredAt
        });

        // Publish event so Logistics service can create an Agent record
        await _publisher.Publish(new DeliveryAgentRegisteredEvent
        {
            AgentId = agent.Id,
            Name = agent.Name,
            Phone = agent.Phone ?? "",
            Email = agent.Email,
            RegisteredAt = agent.RegisteredAt
        });

        var token = GenerateToken(agent);

        return new RegisterDeliveryAgentResult
        {
            Success = true,
            Message = "Delivery Agent registered successfully",
            AgentId = agent.Id,
            Token = token,
            Role = agent.Role,
            Name = agent.Name
        };
    }

    private string GenerateToken(Customer agent)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, agent.Id.ToString()),
            new Claim(ClaimTypes.Email, agent.Email),
            new Claim(ClaimTypes.Name, agent.Name),
            new Claim(ClaimTypes.Role, agent.Role)
        };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(24),
            claims: claims,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
