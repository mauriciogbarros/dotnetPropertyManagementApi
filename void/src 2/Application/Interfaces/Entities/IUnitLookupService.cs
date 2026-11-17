using Domain.Entities;

namespace Application.Interfaces;

public interface IUnitService
{
	Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken ct);
	Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct);
}