using System.ComponentModel.DataAnnotations;

namespace FloodDamageAssessment.Models;

public class User
{
    [Key]
    public string Id { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
