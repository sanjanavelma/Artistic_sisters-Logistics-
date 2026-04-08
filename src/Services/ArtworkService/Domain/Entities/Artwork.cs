namespace ArtworkService.Domain.Entities;
public class Artwork
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // Portrait, Landscape, Abstract, Pet, Couple, Family
    public string ArtworkType { get; set; } = string.Empty;
    // Pencil, Watercolor, Oil, Acrylic, Digital, Charcoal
    public string Medium { get; set; } = string.Empty;
    // A4, A3, 16x20 inches, 24x36 inches
    public string Dimensions { get; set; } = string.Empty;
    // Unique artwork code e.g. "ART-001"
    public string ArtworkCode { get; set; } = string.Empty;
    // URL to artwork photo
    public string ImageUrl { get; set; } = string.Empty;
    // How many available (1 for originals, more for prints)
    public int AvailableQuantity { get; set; }
    // Can this be ordered as custom commission
    public bool IsCustomizable { get; set; } = false;
    public bool IsActive { get; set; } = true;
    // Days to complete after order (for commissions)
    public int EstimatedCompletionDays { get; set; } = 7;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
