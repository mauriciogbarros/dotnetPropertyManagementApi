using Domain.Enums;

namespace Domain.Entities.Users;

public sealed class Tenant : User
{
	public Unit? Unit { get; set; } = null;
	public DateTime MovedIn { get; set; }
	public DateTime? MovedOut { get; set; } = null;

	public Tenant()
	{
		Role = UserRole.Tenant;
	}
}