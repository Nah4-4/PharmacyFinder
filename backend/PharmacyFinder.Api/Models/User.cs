using System.ComponentModel.DataAnnotations;

namespace PharmacyFinder.Api.Models;

public class User
{
    public int Id { set; get; }
    [Required, MaxLength(100)]
    public string FullName { get; set; }=string.Empty;
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Pharmacy> Pharmacies { get; set; } = new List<Pharmacy>();
}
