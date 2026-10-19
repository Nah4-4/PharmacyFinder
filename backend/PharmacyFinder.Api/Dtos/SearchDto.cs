namespace PharmacyFinder.Api.DTOs;

public class MedicineSearchResultDto
{
    public int StockId { get; set; }
    public int MedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool InStock => Quantity > 0;

    public int PharmacyId { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string OperatingHours { get; set; } = string.Empty;
    public double? DistanceKm { get; set; }
}

public class PharmacyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string OperatingHours { get; set; } = string.Empty;
    public double? DistanceKm { get; set; }
}