using System.ComponentModel.DataAnnotations;

namespace PharmacyFinder.Api.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    [Required, MaxLength(100)]
    public required string Action { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public required string EntityName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
