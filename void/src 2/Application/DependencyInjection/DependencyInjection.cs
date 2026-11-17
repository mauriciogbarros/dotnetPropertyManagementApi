using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IManagerService, ManagerService>();
		services.AddScoped<ITechnicianService, TechnicianService>();
		services.AddScoped<ITenantService, TenantService>();

		return services;
	}
}