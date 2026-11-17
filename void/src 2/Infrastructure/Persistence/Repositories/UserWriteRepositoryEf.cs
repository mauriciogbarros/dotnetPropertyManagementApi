using Domain.Abstractions;
using Domain.Entities.Users;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserWriteRepositoryEf : IUserWriteRepository
{
	private readonly AppDbContext _db;

	public UserWriteRepositoryEf(AppDbContext db)
	{
		_db = db;
	}

	public Task AddAsync(User user, CancellationToken ct)
	{
		_db.Users.Add(user);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(User user, CancellationToken ct)
	{
		_db.Users.Remove(user);
		return Task.CompletedTask;
	}

	public Task SaveChangesAsync(CancellationToken ct)
	{
		return _db.SaveChangesAsync(ct);
	}
}