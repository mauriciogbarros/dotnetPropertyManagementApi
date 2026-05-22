using Domain.Entities;

namespace Application.Interfaces;

public interface IUnitLookupService
{
	Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken ct);
	Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct);
}