using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UnitLookupEf : IUnitLookup
{
	private readonly AppDbContext _db;

	public UnitLookupEf(AppDbContext db)
	{
		_db = db;
	}

	public Task<Unit?> GetByIdAsync(Guid unitId, CancellationToken ct)
	{
		return _db.Units.Include(u => u.Tenant)
			.FirstOrDefaultAsync(u => u.Id == unitId, ct);
	}

	public Task<bool> IsUnitOccupiedAsync(Guid unitId, CancellationToken ct)
	{
		return _db.Units.AnyAsync(u => u.Id == unitId && u.Tenant != null, ct);
	}
}