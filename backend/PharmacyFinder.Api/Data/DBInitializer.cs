using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmacyFinder.Api.Enums;
using PharmacyFinder.Api.Models;

namespace PharmacyFinder.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(PFContext context, UserManager<User> userManager)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            UserName = "admin@pharmacyfinder.com",
            FullName = "System Administrator",
            Email = "admin@pharmacyfinder.com",
            Role = UserRole.Admin,
        };

        var owner1 = new User
        {
            UserName = "owner1@pharmacyfinder.com",
            FullName = "Abebe Kebede",
            Email = "owner1@pharmacyfinder.com",
            Role = UserRole.PharmacyOwner,
        };

        var owner2 = new User
        {
            UserName = "owner2@pharmacyfinder.com",
            FullName = "Sara Tadesse",
            Email = "owner2@pharmacyfinder.com",
            Role = UserRole.PharmacyOwner,
        };

        var customer = new User
        {
            UserName = "customer@pharmacyfinder.com",
            FullName = "Test Customer",
            Email = "customer@pharmacyfinder.com",
            Role = UserRole.Customer,
        };

        foreach (var user in new[] { admin, owner1, owner2, customer })
        {
            var result = await userManager.CreateAsync(user, "Password123");
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Unable to seed user {user.Email}: " +
                    string.Join(", ", result.Errors.Select(error => error.Description)));
        }

        var medicines = new List<Medicine>
        {
            new() { Name = "Amoxicillin", DosageType = "500 mg capsule" },
            new() { Name = "Paracetamol", DosageType = "500 mg tablet" },
            new() { Name = "Advil", DosageType = "200 mg tablet" },
            new() { Name = "Zithromax", DosageType = "250 mg tablet" },
            new() { Name = "Omeprazole", DosageType = "300 mg injection" },
            new() { Name = "Metformin", DosageType = "500 mg tablet" },
            new() { Name = "Cipro", DosageType = "500 mg tablet" },
            new() { Name = "Zyrtec", DosageType = "10 mg tablet" },
        };

        context.Medicines.AddRange(medicines);
        await context.SaveChangesAsync();

        var pharmacies = new List<Pharmacy>
        {
            new()
            {
                OwnerId = owner1.Id,
                Name = "Kenema Pharmacy - Bole Branch",
                Address = "Bole Medhanialem, Cameroon St, Addis Ababa",
                Contact = "+251911223344",
                LicenseNumber = "ETH-AA-PH-1001",
                Latitude = 8.9976,
                Longitude = 38.7868,
                OperatingHours = "24 Hours",
                ApprovalStatus = ApprovalStatus.Approved,
            },
            new()
            {
                OwnerId = owner1.Id,
                Name = "Gishen Pharmacy - Kazanchis",
                Address = "Kazanchis, Tito St, Addis Ababa",
                Contact = "+251911556677",
                LicenseNumber = "ETH-AA-PH-1002",
                Latitude = 9.0158,
                Longitude = 38.7681,
                OperatingHours = "08:00 AM - 10:00 PM",
                ApprovalStatus = ApprovalStatus.Approved,
            },
            new()
            {
                OwnerId = owner2.Id,
                Name = "Life Care Pharmacy - Megenagna",
                Address = "Megenagna Roundabout, Addis Ababa",
                Contact = "+251922334455",
                LicenseNumber = "ETH-AA-PH-1003",
                Latitude = 9.0205,
                Longitude = 38.8024,
                OperatingHours = "07:30 AM - 09:30 PM",
                ApprovalStatus = ApprovalStatus.Approved,
            },
            new()
            {
                OwnerId = owner2.Id,
                Name = "Red Cross Pharmacy - Mexico",
                Address = "Mexico Square, Ras Abebe Aregay St, Addis Ababa",
                Contact = "+251933445566",
                LicenseNumber = "ETH-AA-PH-1004",
                Latitude = 9.0104,
                Longitude = 38.7445,
                OperatingHours = "24 Hours",
                ApprovalStatus = ApprovalStatus.Approved,
            },
            new()
            {
                OwnerId = owner2.Id,
                Name = "Pending Test Pharmacy - Piassa",
                Address = "Churchill Ave, Piassa, Addis Ababa",
                Contact = "+251944556677",
                LicenseNumber = "ETH-AA-PH-1005",
                Latitude = 9.0331,
                Longitude = 38.7501,
                OperatingHours = "08:00 AM - 08:00 PM",
                ApprovalStatus = ApprovalStatus.Pending,
            },
        };

        context.Pharmacies.AddRange(pharmacies);
        await context.SaveChangesAsync();

        var stocks = new List<PharmacyStock>
        {
            new()
            {
                PharmacyId = pharmacies[0].Id,
                MedicineId = medicines[0].Id,
                Price = 185.00m,
                Quantity = 45,
            },
            new()
            {
                PharmacyId = pharmacies[0].Id,
                MedicineId = medicines[1].Id,
                Price = 60.00m,
                Quantity = 120,
            },
            new()
            {
                PharmacyId = pharmacies[0].Id,
                MedicineId = medicines[2].Id,
                Price = 95.00m,
                Quantity = 30,
            },
            new()
            {
                PharmacyId = pharmacies[0].Id,
                MedicineId = medicines[3].Id,
                Price = 310.00m,
                Quantity = 15,
            },
            new()
            {
                PharmacyId = pharmacies[0].Id,
                MedicineId = medicines[4].Id,
                Price = 140.00m,
                Quantity = 50,
            },
            new()
            {
                PharmacyId = pharmacies[1].Id,
                MedicineId = medicines[0].Id,
                Price = 165.00m,
                Quantity = 20,
            },
            new()
            {
                PharmacyId = pharmacies[1].Id,
                MedicineId = medicines[1].Id,
                Price = 55.00m,
                Quantity = 80,
            },
            new()
            {
                PharmacyId = pharmacies[1].Id,
                MedicineId = medicines[3].Id,
                Price = 295.00m,
                Quantity = 0,
            },
            new()
            {
                PharmacyId = pharmacies[1].Id,
                MedicineId = medicines[5].Id,
                Price = 220.00m,
                Quantity = 60,
            },
            new()
            {
                PharmacyId = pharmacies[2].Id,
                MedicineId = medicines[0].Id,
                Price = 190.00m,
                Quantity = 12,
            },
            new()
            {
                PharmacyId = pharmacies[2].Id,
                MedicineId = medicines[2].Id,
                Price = 85.00m,
                Quantity = 40,
            },
            new()
            {
                PharmacyId = pharmacies[2].Id,
                MedicineId = medicines[6].Id,
                Price = 250.00m,
                Quantity = 25,
            },
            new()
            {
                PharmacyId = pharmacies[2].Id,
                MedicineId = medicines[7].Id,
                Price = 75.00m,
                Quantity = 90,
            },
            new()
            {
                PharmacyId = pharmacies[3].Id,
                MedicineId = medicines[1].Id,
                Price = 50.00m,
                Quantity = 200,
            },
            new()
            {
                PharmacyId = pharmacies[3].Id,
                MedicineId = medicines[4].Id,
                Price = 130.00m,
                Quantity = 35,
            },
            new()
            {
                PharmacyId = pharmacies[3].Id,
                MedicineId = medicines[5].Id,
                Price = 210.00m,
                Quantity = 40,
            },
            new()
            {
                PharmacyId = pharmacies[3].Id,
                MedicineId = medicines[7].Id,
                Price = 70.00m,
                Quantity = 65,
            },
        };

        context.PharmacyStocks.AddRange(stocks);

        context.AuditLogs.Add(
            new AuditLog
            {
                UserId = admin.Id,
                Action = "SYSTEM_SEED",
                EntityName = "Database",
            }
        );

        await context.SaveChangesAsync();
    }
}
