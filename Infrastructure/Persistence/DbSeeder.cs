using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>
    /// Seeds the development database with:
    /// 1 Property, 2 Managers, 4 Technicians, 8 Units, 5 Tenants (each assigned to a Unit).
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // If already seeded, do nothing.
        if (await db.Properties.AnyAsync(ct))
            return;

        // -----------------------------
        // Property (1)
        // -----------------------------
        var property = new Property
        {
            Name = "Harbor View Apartments",
            Address = new Address
            {
                StreetName = "Seaside Avenue",
                StreetNumber = 12,
                City = "Bremerhaven",
                State = "HB",
                PostalCode = "27568",
                Country = "DE"
            }
        };

        // -----------------------------
        // Units (8)
        // -----------------------------
        var units = new List<Unit>
        {
            new Unit { Property = property, Number = 101, Bedrooms = 1, Bathrooms = 1, AreaM2 = 45.0f, MonthlyRate = 650m, Tenant = null! },
            new Unit { Property = property, Number = 102, Bedrooms = 1, Bathrooms = 1, AreaM2 = 48.0f, MonthlyRate = 680m, Tenant = null! },
            new Unit { Property = property, Number = 103, Bedrooms = 2, Bathrooms = 1, AreaM2 = 62.5f, MonthlyRate = 820m, Tenant = null! },
            new Unit { Property = property, Number = 104, Bedrooms = 2, Bathrooms = 2, AreaM2 = 74.0f, MonthlyRate = 950m, Tenant = null! },
            new Unit { Property = property, Number = 105, Bedrooms = 3, Bathrooms = 2, AreaM2 = 88.0f, MonthlyRate = 1150m, Tenant = null! },
            new Unit { Property = property, Number = 106, Bedrooms = 1, Bathrooms = 1, AreaM2 = 44.0f, MonthlyRate = 640m, Tenant = null! },
            new Unit { Property = property, Number = 107, Bedrooms = 2, Bathrooms = 1, AreaM2 = 60.0f, MonthlyRate = 800m, Tenant = null! },
            new Unit { Property = property, Number = 108, Bedrooms = 3, Bathrooms = 2, AreaM2 = 92.0f, MonthlyRate = 1200m, Tenant = null! }
        };

        // -----------------------------
        // Managers (2)
        // -----------------------------
        var managers = new List<Manager>
        {
            new Manager
            {
                Property = property,
                HourlyRate = 42.50m,
                FirstName = "Ava",
                LastName = "Keller",
                Email = "ava.keller@harborview.dev",
                PhoneNumber = "+49-000-000-0001",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Manager
            {
                Property = property,
                HourlyRate = 45.00m,
                FirstName = "Noah",
                LastName = "Brandt",
                Email = "noah.brandt@harborview.dev",
                PhoneNumber = "+49-000-000-0002",
                HashedPassword = DevPasswordHash("Password123!")
            }
        };

        // -----------------------------
        // Technicians (4)
        // -----------------------------
        var technicians = new List<Technician>
        {
            new Technician
            {
                Property = property,
                HourlyRate = 38.00m,
                Capabilities = new HashSet<TechnicianCapability>
                {
                    TechnicianCapability.Electrical,
                    TechnicianCapability.HVAC,
                    TechnicianCapability.General
                },
                FirstName = "Mia",
                LastName = "Schulz",
                Email = "mia.schulz@harborview.dev",
                PhoneNumber = "+49-000-000-0101",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Technician
            {
                Property = property,
                HourlyRate = 36.00m,
                Capabilities = new HashSet<TechnicianCapability>
                {
                    TechnicianCapability.Plumbing,
                    TechnicianCapability.General
                },
                FirstName = "Liam",
                LastName = "Neumann",
                Email = "liam.neumann@harborview.dev",
                PhoneNumber = "+49-000-000-0102",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Technician
            {
                Property = property,
                HourlyRate = 34.50m,
                Capabilities = new HashSet<TechnicianCapability>
                {
                    TechnicianCapability.Painting,
                    TechnicianCapability.DryWall,
                    TechnicianCapability.General
                },
                FirstName = "Sophia",
                LastName = "Vogel",
                Email = "sophia.vogel@harborview.dev",
                PhoneNumber = "+49-000-000-0103",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Technician
            {
                Property = property,
                HourlyRate = 40.00m,
                Capabilities = new HashSet<TechnicianCapability>
                {
                    TechnicianCapability.Machinery,
                    TechnicianCapability.HVAC
                },
                FirstName = "Ethan",
                LastName = "Fischer",
                Email = "ethan.fischer@harborview.dev",
                PhoneNumber = "+49-000-000-0104",
                HashedPassword = DevPasswordHash("Password123!")
            }
        };

        // -----------------------------
        // Tenants (5) - each assigned to a Unit
        // -----------------------------
        // Assign tenants to the first 5 units, leaving 3 units vacant.
        var tenants = new List<Tenant>
        {
            new Tenant
            {
                Unit = units[0],
                MovedIn = DateTime.UtcNow.AddMonths(-6),
                MovedOut = DateTime.UtcNow.AddYears(1),
                FirstName = "Emma",
                LastName = "Weber",
                Email = "emma.weber@tenant.dev",
                PhoneNumber = "+49-000-000-1001",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Tenant
            {
                Unit = units[1],
                MovedIn = DateTime.UtcNow.AddMonths(-2),
                MovedOut = DateTime.UtcNow.AddYears(1),
                FirstName = "Oliver",
                LastName = "Becker",
                Email = "oliver.becker@tenant.dev",
                PhoneNumber = "+49-000-000-1002",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Tenant
            {
                Unit = units[2],
                MovedIn = DateTime.UtcNow.AddMonths(-10),
                MovedOut = DateTime.UtcNow.AddMonths(8),
                FirstName = "Charlotte",
                LastName = "Hoffmann",
                Email = "charlotte.hoffmann@tenant.dev",
                PhoneNumber = "+49-000-000-1003",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Tenant
            {
                Unit = units[3],
                MovedIn = DateTime.UtcNow.AddMonths(-1),
                MovedOut = DateTime.UtcNow.AddYears(2),
                FirstName = "James",
                LastName = "Kruger",
                Email = "james.kruger@tenant.dev",
                PhoneNumber = "+49-000-000-1004",
                HashedPassword = DevPasswordHash("Password123!")
            },
            new Tenant
            {
                Unit = units[4],
                MovedIn = DateTime.UtcNow.AddMonths(-4),
                MovedOut = DateTime.UtcNow.AddYears(1),
                FirstName = "Amelia",
                LastName = "Hartmann",
                Email = "amelia.hartmann@tenant.dev",
                PhoneNumber = "+49-000-000-1005",
                HashedPassword = DevPasswordHash("Password123!")
            }
        };

        // Set the inverse navigation for occupied units (optional but keeps the object graph consistent)
        foreach (var tenant in tenants)
        {
            tenant.Unit.Tenant = tenant;
        }

        // -----------------------------
        // Persist
        // -----------------------------
        // Add the aggregate graph. EF will cascade insert based on relationships.
        await db.Properties.AddAsync(property, ct);
        await db.Units.AddRangeAsync(units, ct);
        await db.Users.AddRangeAsync(managers, ct);
        await db.Users.AddRangeAsync(technicians, ct);
        await db.Users.AddRangeAsync(tenants, ct);

        await db.SaveChangesAsync(ct);
    }

    // Dev-only "hash" placeholder: deterministic and dependency-free.
    // Swap with your real hashing implementation when you add auth.
    private static string DevPasswordHash(string password)
        => $"DEV_HASH::{password}";
}