namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    InProduction = 2,
    ReadyForDelivery = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6
}
