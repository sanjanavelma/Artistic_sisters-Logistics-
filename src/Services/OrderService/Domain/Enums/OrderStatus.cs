namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Processing,
    ReadyForDispatch,
    Dispatched,
    Delivered,
    Cancelled
}
