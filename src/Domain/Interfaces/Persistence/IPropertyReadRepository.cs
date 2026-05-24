using Domain.Entities;

namespace Domain.Interfaces.Persistence;

public interface IPropertyReadRepository
{
	Task<Property?> GetPropertyByIdAsync(CancellationToken ct);
}