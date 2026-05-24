using Domain.Entities;

namespace Domain.Interfaces.Persistence;

public interface IUnitReadRepository
{
	Task<List<Unit>> GetUnitsAsync(CancellationToken ct);
	Task<Unit?> GetUnitByIdAsync(CancellationToken ct);
	Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct);
}