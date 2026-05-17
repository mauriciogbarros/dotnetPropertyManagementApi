using System;
using System.IO;
using System.Text.Json;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.DesignTime;

/// <summary>
/// Design-time DbContext factory for EF Core tools (migrations, database update, etc.).
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Find a base path that actually contains appsettings.json (handles running dotnet-ef from different folders).
        var basePath = FindNearestAppSettingsBasePath();

        // Determine environment (defaults to Development if not set)
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string \"Default\" was not found. " +
                "Ensure ConnectionStrings:Default exists in appsettings(.{ENV}).json or set ConnectionStrings__Default env var.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // ✅ No duplication: reuse the single source of truth for Npgsql setup (MapEnum, ConfigureDataSource, etc.)
        AppDbContext.ConfigureNpgsql(optionsBuilder, connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string FindNearestAppSettingsBasePath()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (dir != null)
        {
            var appsettingsPath = Path.Combine(dir.FullName, "appsettings.json");
            if (File.Exists(appsettingsPath))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        // Fall back to current directory if appsettings.json isn't found.
        return Directory.GetCurrentDirectory();
    }
}