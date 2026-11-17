using Domain.Entities;

namespace Domain.Abstractions;

public interface IUnitLookup
{
	Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken ct);
	Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct);
}