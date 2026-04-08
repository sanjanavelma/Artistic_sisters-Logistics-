namespace PaymentService.Domain.Enums;

public enum PaymentStatus
{
    // Payment is pending — order just placed
    Pending = 1,

    // Payment confirmed — ready to dispatch
    Confirmed = 2,

    // Payment failed or refunded
    Failed = 3,

    // Refunded after cancellation
    Refunded = 4
}
