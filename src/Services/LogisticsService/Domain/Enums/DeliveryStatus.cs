// All possible states a delivery can be in
// Agent updates this as they move through the delivery process
namespace LogisticsService.Domain.Enums;

public enum DeliveryStatus
{
    // Agent has picked up the order from warehouse
    PickedUp = 1,

    // Order is on the way to dealer
    InTransit = 2,

    // Agent is at dealer's location right now
    OutForDelivery = 3,

    // Successfully handed over to dealer
    Delivered = 4,

    // Delivery attempt failed — dealer not available etc.
    Failed = 5,

    // Assignment cancelled due to Saga compensation/rollback
    Cancelled = 6
}
