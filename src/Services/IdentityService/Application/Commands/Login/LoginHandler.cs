using IdentityService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Application.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IdentityDbContext _db;
    private readonly IConfiguration _config;
    public LoginHandler(IdentityDbContext db, IConfiguration config)
    { _db = db; _config = config; }
    public async Task<LoginResult> Handle(
        LoginCommand request, CancellationToken ct)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email, ct);
        if (customer == null || !BCrypt.Net.BCrypt.Verify(
            request.Password, customer.PasswordHash))
            return new LoginResult
            { Success = false, Message = "Invalid email or password" };
        if (!customer.IsActive)
            return new LoginResult
            { Success = false, Message = "Account is inactive" };
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
            claims: claims, signingCredentials: creds);
        return new LoginResult
        {
            Success = true,
            Message = "Login successful",
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = customer.Role,
            Name = customer.Name,
            CustomerId = customer.Id
        };
    }
}
