using Artistic_Sisters.Shared.Events.Identity;
using IdentityService.Application.Commands.Register;
using IdentityService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IdentityService.Tests.Commands;

/// <summary>
/// Unit tests for <see cref="RegisterHandler"/>.
/// </summary>
public class RegisterHandlerTests : IDisposable
{
    private readonly IdentityDbContext _db;
    private readonly Mock<IPublishEndpoint> _publisherMock;
    private readonly IConfiguration _config;

    public RegisterHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options);

        _publisherMock = new Mock<IPublishEndpoint>();

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "TestSuperSecretKeyThatIsLongEnough_32Chars!"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: build a valid register command
    // ─────────────────────────────────────────────────────────────────────────
    private static RegisterCommand ValidCommand(string email = "new@test.com") => new()
    {
        Name     = "Sanjana",
        Email    = email,
        Password = "Pass@123",
        Phone    = "9876543210",
        Address  = "Hyderabad, India"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 – New email → registration succeeds
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_NewEmail_ReturnsSuccess()
    {
        // Arrange
        var handler = new RegisterHandler(_db, _publisherMock.Object, _config);
        var command = ValidCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful", result.Message);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.CustomerId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 – Duplicate email → registration fails
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange – first registration seeds the email
        var handler = new RegisterHandler(_db, _publisherMock.Object, _config);
        await handler.Handle(ValidCommand("dup@test.com"), CancellationToken.None);

        // Act – second registration with same email
        var result = await handler.Handle(ValidCommand("dup@test.com"), CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Email already registered", result.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 – Customer is persisted in the database after registration
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_NewEmail_PersistsCustomerInDatabase()
    {
        // Arrange
        var handler = new RegisterHandler(_db, _publisherMock.Object, _config);
        var command = ValidCommand("persist@test.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _db.Customers.FirstOrDefaultAsync(c => c.Email == "persist@test.com");
        Assert.NotNull(saved);
        Assert.Equal("Sanjana", saved!.Name);
        Assert.True(saved.IsActive);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 – CustomerRegisteredEvent is published after successful registration
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_NewEmail_PublishesCustomerRegisteredEvent()
    {
        // Arrange
        var handler = new RegisterHandler(_db, _publisherMock.Object, _config);
        var command = ValidCommand("event@test.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert – Publish was called exactly once with a CustomerRegisteredEvent
        _publisherMock.Verify(
            p => p.Publish(It.IsAny<CustomerRegisteredEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 – Password is stored as a BCrypt hash (not plain text)
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_NewEmail_StoresPasswordAsHash()
    {
        // Arrange
        const string plainPassword = "Pass@123";
        var handler = new RegisterHandler(_db, _publisherMock.Object, _config);
        var command = ValidCommand("hash@test.com");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await _db.Customers.FirstOrDefaultAsync(c => c.Email == "hash@test.com");
        Assert.NotNull(saved);
        Assert.NotEqual(plainPassword, saved!.PasswordHash);        // not plain text
        Assert.True(BCrypt.Net.BCrypt.Verify(plainPassword, saved.PasswordHash));  // hash is valid
    }

    public void Dispose() => _db.Dispose();
}
