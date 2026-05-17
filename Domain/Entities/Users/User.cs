using Domain.Enums;

namespace Domain.Entities.Users;

public abstract class User : BaseEntity
{
	public UserRole Role { get; init; }
	public string FirstName { get; set; } = default!;
	public string LastName { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string PhoneNumber { get; set; } = default!;
	public string HashedPassword { get; set; } = default!;
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}