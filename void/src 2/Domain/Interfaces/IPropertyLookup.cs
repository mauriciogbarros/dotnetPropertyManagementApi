using Domain.Entities;

namespace Domain.Abstractions;

public interface IPropertyLookup
{
	Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken ct);
}