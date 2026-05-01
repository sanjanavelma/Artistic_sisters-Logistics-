using IdentityService.Application.Commands.Login;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IdentityService.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="LoginHandler"/>.
/// Uses EF Core InMemory database to avoid real SQL connections.
/// </summary>
public class LoginHandlerTests : IDisposable
{
    private readonly IdentityDbContext _db;
    private readonly IConfiguration _config;

    public LoginHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options);

        // Provide a real-looking JWT secret so token generation succeeds
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "TestSuperSecretKeyThatIsLongEnough_32Chars!"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: seed a customer with a BCrypt-hashed password
    // ─────────────────────────────────────────────────────────────────────────
    private Customer SeedCustomer(string email, string plainPassword, bool isActive = true)
    {
        var customer = new Customer
        {
            Id           = Guid.NewGuid(),
            Name         = "Test User",
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
            Phone        = "9876543210",
            Address      = "123 Art Street",
            IsActive     = isActive,
            Role         = "Customer"
        };
        _db.Customers.Add(customer);
        _db.SaveChanges();
        return customer;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 – Valid credentials → login succeeds with a JWT token
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var email    = "artist@test.com";
        var password = "Secret@123";
        SeedCustomer(email, password);

        var handler = new LoginHandler(_db, _config);
        var command = new LoginCommand { Email = email, Password = password };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token!);
        Assert.Equal("Customer", result.Role);
        Assert.Equal(email, result.Email);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 – Wrong password → login fails
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        // Arrange
        SeedCustomer("user@test.com", "CorrectPass");

        var handler = new LoginHandler(_db, _config);
        var command = new LoginCommand { Email = "user@test.com", Password = "WrongPass" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Token);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 – Email not found → login fails
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_EmailNotFound_ReturnsFailure()
    {
        // Arrange – no customer seeded
        var handler = new LoginHandler(_db, _config);
        var command = new LoginCommand { Email = "nobody@test.com", Password = "Pass" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 – Inactive account → login blocked
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_InactiveAccount_ReturnsAccountInactiveMessage()
    {
        // Arrange
        SeedCustomer("inactive@test.com", "Pass@123", isActive: false);

        var handler = new LoginHandler(_db, _config);
        var command = new LoginCommand { Email = "inactive@test.com", Password = "Pass@123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Account is inactive", result.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 – Successful login returns the correct CustomerId
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsCorrectCustomerId()
    {
        // Arrange
        var seeded  = SeedCustomer("id@test.com", "IdPass@123");
        var handler = new LoginHandler(_db, _config);
        var command = new LoginCommand { Email = "id@test.com", Password = "IdPass@123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(seeded.Id, result.CustomerId);
    }

    public void Dispose() => _db.Dispose();
}
