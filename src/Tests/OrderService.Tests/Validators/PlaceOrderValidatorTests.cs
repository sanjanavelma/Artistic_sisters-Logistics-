using FluentValidation.TestHelper;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Domain.Enums;
using Xunit;

namespace OrderService.Tests.Validators;

/// <summary>
/// Unit tests for <see cref="PlaceOrderValidator"/> using FluentValidation's TestHelper.
/// </summary>
public class PlaceOrderValidatorTests
{
    private readonly PlaceOrderValidator _validator = new();

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 – Valid command passes all rules
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_ValidCommand_HasNoValidationErrors()
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<PlaceOrderItem>
            {
                new() { ArtworkId = Guid.NewGuid(), ArtworkTitle = "Art", Quantity = 1, UnitPrice = 100 }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 – Empty CustomerId → validation error on CustomerId
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_EmptyCustomerId_HasValidationError()
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.Empty,       // invalid
            Items = new List<PlaceOrderItem>
            {
                new() { ArtworkId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 }
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CustomerId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 – Empty Items list → validation error on Items
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_EmptyItems_HasValidationError()
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<PlaceOrderItem>()  // empty
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 – Item with Quantity = 0 → validation error
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_ItemWithZeroQuantity_HasValidationError()
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<PlaceOrderItem>
            {
                new() { ArtworkId = Guid.NewGuid(), Quantity = 0, UnitPrice = 100 }  // invalid
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 – Item with UnitPrice = 0 → validation error
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Validate_ItemWithZeroUnitPrice_HasValidationError()
    {
        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<PlaceOrderItem>
            {
                new() { ArtworkId = Guid.NewGuid(), Quantity = 1, UnitPrice = 0 }  // invalid
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
