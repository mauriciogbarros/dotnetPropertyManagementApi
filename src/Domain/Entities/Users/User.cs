using Domain.Enums;

namespace Domain.Entities.Users;

public abstract class User : BaseEntity
{
	public UserRole Role { get; protected init; }
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public string HashedPassword { get; set; } = string.Empty;
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}