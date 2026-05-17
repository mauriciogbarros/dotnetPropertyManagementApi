namespace Domain.ValueObjects;

public record Address
{
	public string StreetName { get; init; } = default!;
	public int StreetNumber { get; init; } = default!;
	public string City { get; init; } = default!;
	public string State { get; init; } = default!;
	public string PostalCode { get; init; } = default!;
	public string Country { get; init; } = default!;

	public Address() { }
}