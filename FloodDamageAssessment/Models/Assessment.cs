using System.ComponentModel.DataAnnotations;

namespace FloodDamageAssessment.Models;

public class Assessment
{
    [Key]
    public string Id { get; set; } = null!;

    [Required(ErrorMessage = "Site name is required.")]
    public string? SiteName { get; set; }

    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double? Lat { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double? Lng { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Condition is required.")]
    [RegularExpression("^(Good|Moderate|Bad)$", ErrorMessage = "Condition must be Good, Moderate, or Bad.")]
    public string? Condition { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Chickens must be zero or greater.")]
    public int Chickens { get; set; }

    public string? PhotoUrlsJson { get; set; }
    public string? Assessor { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
