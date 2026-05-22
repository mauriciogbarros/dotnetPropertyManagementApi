using Domain.Abstractions;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence.DependencyInjection;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddScoped<IPropertyLookup, PropertyLookupEf>();
		services.AddScoped<IUnitLookup, UnitLookupEf>();
		services.AddScoped<IUserReadRepository, UserReadRepositoryEf>();
		services.AddScoped<IUserWriteRepository, UserWriteRepositoryEf>();

		return services;
	}
}