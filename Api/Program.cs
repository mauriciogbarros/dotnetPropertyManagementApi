using Application.DependencyInjection;
using Infrastructure.Persistence;
using Infrastructure.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Build: Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application layer (use-cases)
builder.Services.AddApplication();

// Infrastructure layer
// - EF adapters
// - Repositories implementing Domain ports
builder.Services.AddInfrastructure();

// DbContext
// - Enum mapping/config is inside Infrastructure via
//   AppDbContext.ConfigureNpgsql
var connString = builder.Configuration.GetConnectionString("Default") ??
	throw new InvalidOperationException("Missing connection string: \"Default\".");

builder.Services.AddDbContext<AppDbContext>(options => AppDbContext.ConfigureNpgsql(options, connString));

var app = builder.Build();

// Middleware (run phase)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	// Apply migrations + seed dev data on startup
	// Minimal-hosting pattern:
	// - Create a scope
	// - Resolve DbContext
	// - Run Migrate()
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	await db.Database.MigrateAsync();
	await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();