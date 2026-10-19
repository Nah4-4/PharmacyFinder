using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using PharmacyFinder.Api.Enums;

namespace PharmacyFinder.Api.Models;

public class User : IdentityUser<int>
{
    [Required, MaxLength(100)]
    public string FullName { get; set; }=string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Pharmacy> Pharmacies { get; set; } = new List<Pharmacy>();
}
