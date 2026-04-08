using System;

namespace OrderService.Domain.Entities;

public class CustomCommissionOrder : Order
{
    public string ArtworkType { get; set; } = string.Empty;
    public string Medium { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string ReferencePhotoUrl { get; set; } = string.Empty;
    public string SpecialInstructions { get; set; } = string.Empty;
}
