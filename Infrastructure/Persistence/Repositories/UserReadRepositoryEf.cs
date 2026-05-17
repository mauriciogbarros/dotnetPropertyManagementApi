using Domain.Abstractions;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserReadRepositoryEf : IUserReadRepository
{
	private readonly AppDbContext _db;

	public UserReadRepositoryEf(AppDbContext db)
	{
		_db = db;
	}

	public Task<List<Manager>> GetManagersAsync(CancellationToken ct)
	{
		return _db.Managers.Include(m => m.Property)
			.ToListAsync(ct);
	}

	public Task<Manager?> GetManagerByIdAsync(Guid id, CancellationToken ct)
	{
		return _db.Managers.Include(m => m.Property)
			.FirstOrDefaultAsync(m => m.Id == id, ct);
	}

	public Task<List<Technician>> GetTechniciansAsync(CancellationToken ct)
	{
		return _db.Technicians.Include(t => t.Property)
			.ToListAsync(ct);
	}

	public Task<Technician?> GetTechnicianByIdAsync(Guid id, CancellationToken ct)
	{
		return _db.Technicians.Include(t => t.Property)
			.FirstOrDefaultAsync(t => t.Id == id, ct);
	}

	public Task<List<Tenant>> GetTenantsAsync(CancellationToken ct)
	{
		return _db.Tenants.Include(t => t.Unit).ToListAsync(ct);
	}

	public Task<Tenant?> GetTenantByIdAsync(Guid id, CancellationToken ct)
	{
		return _db.Tenants.Include(t => t.Unit).FirstOrDefaultAsync(t => t.Id == id, ct);
	}

}