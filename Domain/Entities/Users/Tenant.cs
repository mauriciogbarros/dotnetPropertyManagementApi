using Domain.Enums;

namespace Domain.Entities.Users;

public sealed class Tenant : User
{
	public Unit Unit { get; set; } = default!;
	public DateTime MovedIn { get; set; } = default!;
	public DateTime MovedOut { get; set; } = default!;

	public Tenant()
	{
		Role = UserRole.Tenant;
	}

}