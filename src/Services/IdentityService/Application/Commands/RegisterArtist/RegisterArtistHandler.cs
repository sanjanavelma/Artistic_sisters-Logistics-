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

namespace IdentityService.Application.Commands.RegisterArtist;

public class RegisterArtistHandler : IRequestHandler<RegisterArtistCommand, RegisterArtistResult>
{
    private readonly IdentityDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly IConfiguration _config;
    
    public RegisterArtistHandler(IdentityDbContext db,
        IPublishEndpoint publisher, IConfiguration config)
    {
        _db = db; _publisher = publisher; _config = config;
    }
    
    public async Task<RegisterArtistResult> Handle(
        RegisterArtistCommand request, CancellationToken ct)
    {
        var exists = await _db.Customers
            .AnyAsync(c => c.Email == request.Email, ct);
        if (exists)
            return new RegisterArtistResult
            { Success = false, Message = "Email already registered" };
            
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Address = request.Address,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow,
            Role = "Artist" // FORCE ROLE TO ARTIST
        };
        
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        
        await _publisher.Publish(new CustomerRegisteredEvent
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            RegisteredAt = customer.RegisteredAt
        });
        
        var token = GenerateToken(customer);
        
        return new RegisterArtistResult
        {
            Success = true,
            Message = "Artist Registration successful",
            CustomerId = customer.Id,
            Token = token
        };
    }
    
    private string GenerateToken(Customer customer)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
            
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Name, customer.Name),
            new Claim(ClaimTypes.Role, customer.Role)
        };
        
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(24),
            claims: claims,
            signingCredentials: creds);
            
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
