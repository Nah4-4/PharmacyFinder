using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyFinder.Api.Models;

public class Medicine
{
    public int Id { get; set; }
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(100)]
    public string DosageType {get; set;}=string.Empty;
    public ICollection<PharmacyStock> PharmacyStocks { get; set; } = new List<PharmacyStock>();
}
