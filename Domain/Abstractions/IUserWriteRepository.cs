using Domain.Entities.Users;

namespace Domain.Abstractions;

public interface IUserWriteRepository
{
	Task AddAsync(User user, CancellationToken ct);
	Task DeleteAsync(User user, CancellationToken ct);
	Task SaveChangesAsync(CancellationToken ct);
}