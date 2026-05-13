
using Domain.Enums;

namespace Domain.Entities.Users;

public sealed class Technician : User
{
	public Property Property { get; init; } = default!;
	public HashSet<TechnicianCapability> Capabilities { get; set; } = [];

	public Technician()
	{
		Role = UserRole.Technician;
	}
}