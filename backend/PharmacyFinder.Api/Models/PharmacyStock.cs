using Microsoft.EntityFrameworkCore;

namespace PharmacyFinder.Api.Models;

public class PharmacyStock
{
    public int Id { get; set; }
    public int PharmacyId { get; set; }
    public Pharmacy Pharmacy { get; set; } = null!;

    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;

    [Precision(18,2)]
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
