using Domain.Entities.Users;

namespace Domain.Entities;

public sealed class Unit : BaseEntity
{
	public Property Property { get; init; } = default!;
	public Tenant Tenant { get; set; } = default!;
	public int Number { get; set; } = default!;
	public int Bedrooms { get; set; } = default!;
	public int Bathrooms { get; set; } = default!;
	public float AreaM2 { get; set; } = default!;
	public decimal MonthlyRate { get; set; } = default;

	public Unit() { }
}