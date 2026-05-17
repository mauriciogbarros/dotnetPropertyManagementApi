using Application.Dtos;

namespace Application.Abstractions;

public interface ITenantService
{
	Task<List<TenantDto>> GetAllAsync(CancellationToken ct);
	Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct);
	Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken ct);
	Task<bool> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct);
	Task<bool> AssignUnitAsync(Guid id, AssignTenantUnitRequest request, CancellationToken ct);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}