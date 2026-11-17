using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PropertyLookupEf : IPropertyLookup
{
	private readonly AppDbContext _db;

	public PropertyLookupEf(AppDbContext db)
	{
		_db = db;
	}

	public Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken ct)
	{
		return _db.Properties.FirstOrDefaultAsync(p => p.Id == propertyId, ct);
	}
}