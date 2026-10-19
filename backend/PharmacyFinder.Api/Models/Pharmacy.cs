using System.ComponentModel.DataAnnotations;
using PharmacyFinder.Api.Enums;

namespace PharmacyFinder.Api.Models;

public class Pharmacy
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;
    [Required, MaxLength(30)]
    public string Contact { get; set; } = string.Empty;
    [Required, MaxLength(150)]
    public string LicenseNumber { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string OperatingHours { get; set; } = "08:00 AM - 09:00 PM";
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PharmacyStock> Stocks { get; set; } = new List<PharmacyStock>();
}
