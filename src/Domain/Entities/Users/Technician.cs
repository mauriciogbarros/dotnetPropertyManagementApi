
using Domain.Enums;

namespace Domain.Entities.Users;

public sealed class Technician : User
{
	public Property Property { get; init; } = default!;
	public List<TechnicianCapability> Capabilities { get; set; } = [];
	public decimal HourlyRate { get; set; } = default!;

	public Technician()
	{
		Role = UserRole.Technician;
	}
}