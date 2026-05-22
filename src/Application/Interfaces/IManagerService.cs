using Application.Dtos;

namespace Application.Interfaces;

public interface IManagerService
{
	Task<List<ManagerDto>> GetAllAsync(CancellationToken ct);
	Task<ManagerDto?> GetByIdAsync(Guid id, CancellationToken ct);
	Task<Guid> CreateAsync(CreateManagerRequest request, CancellationToken ct);
	Task<bool> UpdateAsync(Guid id, UpdateManagerRequest request, CancellationToken ct);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}