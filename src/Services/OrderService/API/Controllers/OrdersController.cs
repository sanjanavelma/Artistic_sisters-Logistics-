using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Commands.PlaceCommission;
using OrderService.Application.Commands.UpdateOrderStatus;
using OrderService.Application.Queries.GetCustomerOrders;
using OrderService.Application.Queries.GetAllOrders;
using OrderService.Application.Queries.GetOrderById;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlaceOrderCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("commission")]
    public async Task<IActionResult> PlaceCommission([FromBody] PlaceCommissionCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(Guid customerId)
    {
        var result = await _mediator.Send(new GetCustomerOrdersQuery(customerId));
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        if (result == null) return NotFound(new { message = "Order not found" });
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        var cmd = new UpdateOrderStatusCommand { OrderId = id, Status = req.Status };
        var result = await _mediator.Send(cmd);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

public class UpdateOrderStatusRequest
{
    public OrderService.Domain.Enums.OrderStatus Status { get; set; }
}
