using Domain.Entities.Users;

namespace Domain.Interfaces.Persistence;

public interface IUserReadRepository
{
	Task<List<Manager>> GetManagersAsync(CancellationToken ct);
	Task<Manager?> GetManagerByIdAsync(Guid id, CancellationToken ct);
	Task<List<Technician>> GetTechniciansAsync(CancellationToken ct);
	Task<Technician?> GetTechnicianByIdAsync(CancellationToken ct);
	Task<List<Tenant>> GetTenantsAsync(CancellationToken ct);
	Task<Tenant?> GetTenantByIdAsync(CancellationToken ct);
}