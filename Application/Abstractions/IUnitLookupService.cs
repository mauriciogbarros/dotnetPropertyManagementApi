using Domain.Entities;

namespace Application.Abstractions;

public interface IUnitLookupService
{
	Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken ct);
	Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct);
}