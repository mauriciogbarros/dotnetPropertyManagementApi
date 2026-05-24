using Application.DependencyInjection;
using Application.Interfaces.Authentication;
using Application.Services.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Persistence.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Build: Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication: JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) && !builder.Environment.IsDevelopment())
{
	throw new InvalidOperationException("JWT Key is missing. Please configure \"Jwt:Keyt\" in production.");
}
jwtKey ??= "dev_secret_key_change_me_for_local_dev_only_32_chars";

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "dotnetPropertyManagementApi";

var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "dotnetPropertyManagementApiAudience";

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
		options.RequireHttpsMetadata = false;
		options.SaveToken = true;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,
			ValidateAudience = true,
			ValidAudience = jwtAudience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromMinutes(2)
		};
	});

// Authorization: role-based policies for Manager, Technician and Tenant
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("ManagerOnly", p => p.RequireRole("Manager"));
	options.AddPolicy("TechnicianOnly", p => p.RequireRole("Technician"));
	options.AddPolicy("TenantOnly", p => p.RequireRole("Tenant"));
});

// Password hasher (bcrypt) and token service
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();