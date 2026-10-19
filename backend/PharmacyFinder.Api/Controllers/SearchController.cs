using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyFinder.Api.Data;
using PharmacyFinder.Api.DTOs;
using PharmacyFinder.Api.Enums;
using PharmacyFinder.Api.Models;

namespace PharmacyFinder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly PFContext _context;

    public SearchController(PFContext context)
    {
        _context = context;
    }

    // 1. Search medicines and filter by availability, distance, or price
    [HttpGet("medicines")]
    public async Task<ActionResult<ApiResponse<List<MedicineSearchResultDto>>>> SearchMedicines(
        [FromQuery] string? query,
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] double? maxDistanceKm,
        [FromQuery] bool inStockOnly = false,
        [FromQuery] SearchSortBy sortBy = SearchSortBy.Price)
    {
        var validationError = ValidateLocation(lat, lng, maxDistanceKm);
        if (validationError is not null)
            return BadRequest(ApiResponse<List<MedicineSearchResultDto>>.Fail(validationError));

        if (sortBy == SearchSortBy.Distance && !HasLocation(lat, lng))
        {
            return BadRequest(ApiResponse<List<MedicineSearchResultDto>>.Fail(
                "Latitude and longitude are required when sorting by distance."));
        }

        var stocksQuery = _context.PharmacyStocks
            .AsNoTracking()
            .Where(s => s.Pharmacy.ApprovalStatus == ApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var search = query.Trim();
            stocksQuery = stocksQuery.Where(s =>
                s.Medicine.Name.Contains(search));
        }

        if (inStockOnly)
        {
            stocksQuery = stocksQuery.Where(s => s.Quantity > 0);
        }

        var results = await stocksQuery
            .Select(s => new MedicineSearchResultDto
            {
                StockId = s.Id,
                MedicineId = s.MedicineId,
                Name = s.Medicine.Name,
                Price = s.Price,
                Quantity = s.Quantity,
                PharmacyId = s.PharmacyId,
                PharmacyName = s.Pharmacy.Name,
                Address = s.Pharmacy.Address,
                Contact = s.Pharmacy.Contact,
                Latitude = s.Pharmacy.Latitude,
                Longitude = s.Pharmacy.Longitude,
                OperatingHours=s.Pharmacy.OperatingHours,
                DistanceKm = null
            })
            .ToListAsync();

        if (HasLocation(lat, lng))
        {
            results.ForEach(result =>
                result.DistanceKm = CalculateDistanceKm(
                    lat!.Value,
                    lng!.Value,
                    result.Latitude,
                    result.Longitude));
        }

        if (maxDistanceKm.HasValue && lat.HasValue && lng.HasValue)
        {
            results = results.Where(r => r.DistanceKm <= maxDistanceKm.Value).ToList();
        }

        results = sortBy switch
        {
            SearchSortBy.Distance => results.OrderBy(r => r.DistanceKm).ToList(),
            SearchSortBy.Availability => results.OrderByDescending(r => r.Quantity).ToList(),
            _ => results.OrderBy(r => r.Price).ToList()
        };

        if (results.Count == 0)
        {
            return NotFound(ApiResponse<List<MedicineSearchResultDto>>.Fail(
                "No medicines found matching the search criteria."));
        }

        return Ok(ApiResponse<List<MedicineSearchResultDto>>.Ok(results));
    }

    // 2. Find approved pharmacies by location
    [HttpGet("pharmacies")]
    public async Task<ActionResult<ApiResponse<List<PharmacyDto>>>> GetPharmacies(
        [FromQuery] double? lat,
        [FromQuery] double? lng)
    {
        var validationError = ValidateLocation(lat, lng, null);
        if (validationError is not null)
            return BadRequest(ApiResponse<List<PharmacyDto>>.Fail(validationError));

        var pharmacies = await _context.Pharmacies
            .AsNoTracking()
            .Where(p => p.ApprovalStatus == ApprovalStatus.Approved)
            .Select(p => new PharmacyDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                ContactPhone = p.Contact,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                OperatingHours = p.OperatingHours,
                DistanceKm = null
            })
            .ToListAsync();

        if (HasLocation(lat, lng))
        {
            pharmacies.ForEach(pharmacy =>
                pharmacy.DistanceKm = CalculateDistanceKm(
                    lat!.Value,
                    lng!.Value,
                    pharmacy.Latitude,
                    pharmacy.Longitude));
            pharmacies = pharmacies.OrderBy(pharmacy => pharmacy.DistanceKm).ToList();
        }
        else
        {
            pharmacies = pharmacies.OrderBy(pharmacy => pharmacy.Name).ToList();
        }

        if (pharmacies.Count == 0)
        {
            return NotFound(ApiResponse<List<PharmacyDto>>.Fail(
                "No approved pharmacies found."));
        }

        return Ok(ApiResponse<List<PharmacyDto>>.Ok(pharmacies));
    }

    // 3. Get all medicines in stock for a specific pharmacy
    [HttpGet("pharmacies/{id}/stocks")]
    public async Task<ActionResult<ApiResponse<List<MedicineSearchResultDto>>>> GetPharmacyStocks(int id)
    {
        var pharmacyExists = await _context.Pharmacies
            .AnyAsync(p => p.Id == id && p.ApprovalStatus == ApprovalStatus.Approved);

        if (!pharmacyExists)
            return NotFound(ApiResponse<List<MedicineSearchResultDto>>.Fail("Pharmacy not found."));

        var stocks = await _context.PharmacyStocks
            .AsNoTracking()
            .Include(s => s.Medicine)
            .Include(s => s.Pharmacy)
            .Where(s => s.PharmacyId == id)
            .Select(s => new MedicineSearchResultDto
            {
                StockId = s.Id,
                MedicineId = s.MedicineId,
                Name = s.Medicine.Name,
                Price = s.Price,
                Quantity = s.Quantity,
                PharmacyId = s.PharmacyId,
                PharmacyName = s.Pharmacy.Name,
                Address = s.Pharmacy.Address,
                Contact = s.Pharmacy.Contact,
                Latitude = s.Pharmacy.Latitude,
                Longitude = s.Pharmacy.Longitude,
                OperatingHours = s.Pharmacy.OperatingHours
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MedicineSearchResultDto>>.Ok(stocks));
    }

    // Basic Haversine distance formula in kilometers
    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        a = Math.Clamp(a, 0, 1);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(R * c, 2);
    }

    private static bool HasLocation(double? lat, double? lng) =>
        lat.HasValue && lng.HasValue;

    private static string? ValidateLocation(double? lat, double? lng, double? maxDistanceKm)
    {
        if (lat.HasValue != lng.HasValue)
            return "Latitude and longitude must be provided together.";

        if (maxDistanceKm is < 0)
            return "Maximum distance cannot be negative.";

        if (maxDistanceKm.HasValue && !HasLocation(lat, lng))
            return "Latitude and longitude are required when filtering by distance.";

        return null;
    }
}