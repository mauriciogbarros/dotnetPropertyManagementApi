using Domain.Enums;

namespace Domain.Entities.Users;

public sealed class Manager : User
{
	public Property Property { get; init; } = default!;
	public decimal HourlyRate { get; set; } = default!;

	public Manager()
	{
		Role = UserRole.Manager;
	}
}