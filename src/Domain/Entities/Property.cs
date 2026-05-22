using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class Property : BaseEntity
{
	public string Name { get;  set; } = default!;
	public Address Address { get; set; } = default!;
	public ICollection<User> Users { get; set; } = [];
	public ICollection<Unit> Units { get; set; } = [];
	
	public Property() { }
}