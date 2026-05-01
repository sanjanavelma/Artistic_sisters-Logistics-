using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _config;

    public NotificationController(NotificationDbContext db, IEmailSender email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    // ── POST api/notifications/contact ─────────────────────────────────────────
    // Receives contact form submissions from the website and emails them to the
    // Artistic Sisters inbox using the existing MailKit / Gmail SMTP setup.
    [HttpPost("contact")]
    public async Task<IActionResult> SendContactMessage([FromBody] ContactMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Message))
        {
            return BadRequest(new { success = false, message = "Name, email and message are required." });
        }

        var inboxEmail = _config["Email:SenderEmail"] ?? "artistic.sisters07@gmail.com";
        var subject    = $"Contact Us: {(string.IsNullOrWhiteSpace(req.Subject) ? "Website Enquiry" : req.Subject)}";

        var htmlBody = $@"
<div style='font-family:Arial,sans-serif;max-width:580px;margin:0 auto;'>
  <h2 style='color:#c2848a;border-bottom:2px solid #f7ede8;padding-bottom:12px;'>
    New Message via Artistic Sisters Website
  </h2>
  <table style='width:100%;border-collapse:collapse;font-size:15px;'>
    <tr>
      <td style='padding:8px 0;color:#888;width:90px;vertical-align:top;'><strong>From</strong></td>
      <td style='padding:8px 0;'>{System.Web.HttpUtility.HtmlEncode(req.Name)}</td>
    </tr>
    <tr>
      <td style='padding:8px 0;color:#888;vertical-align:top;'><strong>Email</strong></td>
      <td style='padding:8px 0;'><a href='mailto:{System.Web.HttpUtility.HtmlEncode(req.Email)}' style='color:#c2848a;'>{System.Web.HttpUtility.HtmlEncode(req.Email)}</a></td>
    </tr>
    <tr>
      <td style='padding:8px 0;color:#888;vertical-align:top;'><strong>Subject</strong></td>
      <td style='padding:8px 0;'>{System.Web.HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(req.Subject) ? "—" : req.Subject)}</td>
    </tr>
    <tr>
      <td style='padding:8px 0;color:#888;vertical-align:top;'><strong>Message</strong></td>
      <td style='padding:8px 0;white-space:pre-wrap;'>{System.Web.HttpUtility.HtmlEncode(req.Message)}</td>
    </tr>
  </table>
  <hr style='border:none;border-top:1px solid #f0e8e8;margin:24px 0;' />
  <p style='color:#aaa;font-size:12px;'>Sent via the Artistic Sisters contact form</p>
</div>";

        await _email.SendEmailAsync(inboxEmail, "Artistic Sisters", subject, htmlBody);

        return Ok(new { success = true, message = "Message sent successfully!" });
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

public class ContactMessageRequest
{
    public string Name    { get; set; } = string.Empty;
    public string Email   { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
