namespace ArtworkService.Domain.Entities;
public class NotifyMeSubscription
{
    public Guid Id { get; set; }
    public Guid ArtworkId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public bool NotificationSent { get; set; } = false;
    public Artwork Artwork { get; set; } = null!;
}
