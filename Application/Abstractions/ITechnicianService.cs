using Application.Dtos;

namespace Application.Abstractions;

public interface ITechnicianService
{
	Task<List<TechnicianDto>> GetAllAsync(CancellationToken ct);
	Task<TechnicianDto?> GetByIdAsync(Guid id, CancellationToken ct);
	Task<Guid> CreateAsync(CreateTechnicianRequest request, CancellationToken ct);
	Task<bool> UpdateAsync(Guid id, UpdateTechnicianRequest request, CancellationToken ct);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}