using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.Enums;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
	// Aggregate roots / main sets
	public DbSet<Property> Properties => Set<Property>();
	public DbSet<Unit> Units => Set<Unit>();

	// TPH root set
	public DbSet<User> Users => Set<User>();
	public DbSet<Manager> Managers => Set<Manager>();
	public DbSet<Technician> Technicians => Set<Technician>();
	public DbSet<Tenant> Tenants => Set<Tenant>();

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

	/// <summary>
	/// Centralized Npgsql configuration (including enum mapping) inside the Infrastructure.
	/// Program.cs can call this without referencing Domain types.
	/// </summary>
	public static DbContextOptionsBuilder ConfigureNpgsql(
	DbContextOptionsBuilder optionsBuider,
	string connectionString		
	)
	{
		return optionsBuider.UseNpgsql(connectionString, npgsql =>
		{
			// EF/provider layer enum mapping (drives EF model/migrations)
			npgsql.MapEnum<UserRole>("t_user_role");
			npgsql.MapEnum<TechnicianCapability>("t_technician_capability");

			// ADO.NET layer enum mapping via the NpgsqlDataSourceBuilder
			// (kept inside Infrastructure)
			npgsql.ConfigureDataSource(dataSourceBuilder =>
			{
				dataSourceBuilder.MapEnum<UserRole>("t_user_role");
				dataSourceBuilder.MapEnum<TechnicianCapability>("t_technician_capability");
			});
		});
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Property>()
			.Property(e => e.Id)
			.ValueGeneratedNever();

		modelBuilder.Entity<Unit>()
			.Property(e => e.Id)
			.ValueGeneratedNever();

		modelBuilder.Entity<User>()
			.Property(e => e.Id)
			.ValueGeneratedNever();

		// Property
		modelBuilder.Entity<Property>(e =>
		{
			e.ToTable("Properties");
			e.HasKey(p => p.Id);
			e.Property(p => p.Name)
				.IsRequired()
				.HasColumnName("name");
			e.OwnsOne(p => p.Address, a =>
			{
				a.Property(x => x.StreetName)
					.IsRequired()
					.HasColumnName("street_name");
				a.Property(x => x.StreetNumber)
					.IsRequired()
					.HasColumnName("street_number");
				a.Property(x => x.City)
					.IsRequired()
					.HasColumnName("city");
				a.Property(x => x.State)
					.IsRequired()
					.HasColumnName("state");
				a.Property(x => x.PostalCode)
					.IsRequired()
					.HasColumnName("postal_code");
				a.Property(x => x.Country)
					.IsRequired()
					.HasColumnName("country");
			});

			e.HasMany(p => p.Units)
				.WithOne(u => u.Property)
				.HasForeignKey("property_id")
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			e.HasMany(p => p.Users)
				.WithOne()
				.HasForeignKey("property_id")
				.OnDelete(DeleteBehavior.Restrict);
		});

		// Unit
		modelBuilder.Entity<Unit>(e =>
		{
			e.ToTable("Units");
			e.HasKey(u => u.Id);
			e.Property(u => u.Number)
				.IsRequired()
				.HasColumnName("number");
			e.Property(u => u.Bedrooms)
				.IsRequired()
				.HasColumnName("bedrooms");
			e.Property(u => u.Bathrooms)
				.IsRequired()
				.HasColumnName("bathrooms");
			e.Property(u => u.AreaM2)
				.IsRequired()
				.HasColumnName("area_mm2");
			e.Property(u => u.MonthlyRate)
				.HasPrecision(12, 2)
				.IsRequired()
				.HasColumnName("monthly_rate");
			// 1:1 Unit <-> Tenant (FK lives on Tenant row in Users table).
			e.HasOne(u => u.Tenant)
				.WithOne(t => t.Unit)
				.HasForeignKey<Tenant>("UnitId")
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
		});

		// User
		// Inhertiance (TPH)
		modelBuilder.Entity<User>(e =>
		{
			e.ToTable("Users");
			e.HasKey(u => u.Id);
			e.Property(u => u.FirstName)
				.IsRequired()
				.HasColumnName("first_name");
			e.Property(u => u.LastName)
				.IsRequired()
				.HasColumnName("last_name");
			e.Property(u => u.Email)
				.IsRequired()
				.HasColumnName("email");
			e.Property(u => u.PhoneNumber)
				.IsRequired()
				.HasColumnName("phone_number");
			e.Property(u => u.HashedPassword)
				.IsRequired()
				.HasColumnName("hashed_password");
			e.Property(u => u.CreatedAt)
				.IsRequired()
				.HasColumnName("created_at");
			// IMPORTANT: Role is now native Postgre enum type "t_user_role"
			e.Property(u => u.Role)
				.HasColumnType("t_user_role")
				.IsRequired();
			// TPH discriminator configured on Role (enum).
			e.HasDiscriminator<UserRole>(u => u.Role)
				.HasValue<Manager>(UserRole.Manager)
				.HasValue<Technician>(UserRole.Technician)
				.HasValue<Tenant>(UserRole.Tenant);
			e.HasIndex(u => u.Email).IsUnique();
		});

		// Manager (TPH Columns)
		modelBuilder.Entity<Manager>(e =>
		{
			e.Property(m => m.HourlyRate)
				.HasPrecision(10, 2)
				.IsRequired()
				.HasColumnName("hourly_rate");
			e.HasOne(m => m.Property)
				.WithMany()
				.HasForeignKey("PropertyId")
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
		});

		// Technician (TPH Columns + native enum array)
		modelBuilder.Entity<Technician>(e =>
		{
			e.Property(t => t.HourlyRate)
				.HasPrecision(10, 2)
				.IsRequired()
				.HasColumnName("hourly_rate");
			e.HasOne(t => t.Property)
				.WithMany()
				.HasForeignKey("PropertyId")
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);

			// IMPORTANT: Capabilities is stored as native PG enum array
			// "t_technician_capability[]".
			// Domain type is TechnicianCapability[]
			e.Property(t => t.Capabilities)
				.HasColumnType("t_technician_capability[]");

			// Ensure PostgreSQL array column type
			e.Property(t => t.Capabilities).HasColumnType("text[]");
		});

		// Tenant (TPH Columns)
		modelBuilder.Entity<Tenant>(e =>
		{
			e.Property(t => t.MovedIn)
				.IsRequired()
				.HasColumnName("moved_in");
			e.Property(t => t.MovedOut)
				.IsRequired()
				.HasColumnName("moved_out");
			// FK column in Users for the Unit relationship
			e.Property<Guid>("UnitId");
			e.HasIndex("UnitId").IsUnique();
		});
	}
}