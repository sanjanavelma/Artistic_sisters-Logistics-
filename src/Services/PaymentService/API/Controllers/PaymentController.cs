using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _db;

    public PaymentController(PaymentDbContext db)
    {
        _db = db;
    }

    // GET api/payment/order/{orderId} — get payment status for an order
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrder(Guid orderId)
    {
        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

        if (payment == null)
            return NotFound(new { message = "Payment not found" });

        return Ok(new
        {
            payment.OrderId,
            payment.Amount,
            Status = payment.Status.ToString(),
            payment.PaymentMode,
            payment.CreatedAt,
            payment.ConfirmedAt
        });
    }

    // GET api/payment/saga/{orderId} — get saga state for an order
    [HttpGet("saga/{orderId}")]
    public async Task<IActionResult> GetSagaState(Guid orderId)
    {
        var saga = await _db.DispatchSagas
            .FirstOrDefaultAsync(s => s.OrderId == orderId);

        if (saga == null)
            return NotFound(new { message = "No saga found for this order" });

        return Ok(new
        {
            saga.OrderId,
            State = saga.State.ToString(),
            saga.PaymentLocked,
            saga.AgentAssigned,
            saga.StartedAt,
            saga.CompletedAt,
            saga.FailureReason
        });
    }
}
