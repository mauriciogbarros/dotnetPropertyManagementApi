using Domain.Entities.Users;

namespace Domain.Abstractions;

public interface IUserReadRepository
{
	Task<List<Manager>> GetManagersAsync(CancellationToken ct);
	Task<Manager?> GetManagerByIdAsync(Guid id, CancellationToken ct);
	Task<List<Technician>> GetTechniciansAsync(CancellationToken ct);
	Task<Technician?> GetTechnicianByIdAsync(Guid id, CancellationToken ct);
	Task<List<Tenant>> GetTenantsAsync(CancellationToken ct);
	Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken ct);
}