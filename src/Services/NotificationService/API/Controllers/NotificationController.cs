using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _db;

    public NotificationController(NotificationDbContext db)
    {
        _db = db;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _db.NotificationLogs
            .OrderByDescending(l => l.SentAt)
            .Take(50)
            .Select(l => new
            {
                l.EventType,
                l.RecipientEmail,
                l.Subject,
                l.IsSuccess,
                l.ErrorMessage,
                l.SentAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _db.EmailTemplates
            .Where(t => t.IsActive)
            .ToListAsync();

        return Ok(templates);
    }
}
