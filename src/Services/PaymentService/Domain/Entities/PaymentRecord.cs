// Tracks payment status for each order
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public class PaymentRecord
{
    public Guid Id { get; set; }

    // Which order this payment is for
    public Guid OrderId { get; set; }

    // Which customer is paying
    public Guid CustomerId { get; set; }

    // Total amount to pay
    public decimal Amount { get; set; }

    // Current payment status
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // COD or Online
    public string PaymentMode { get; set; } = string.Empty;

    // Razorpay Order ID
    public string? ProviderOrderId { get; set; }

    // Razorpay Payment ID (after success)
    public string? ProviderPaymentId { get; set; }

    // When payment record was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // When payment was confirmed
    public DateTime? ConfirmedAt { get; set; }
}
