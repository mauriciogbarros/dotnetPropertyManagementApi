namespace Domain.ValueObjects;

public record Address
{
	public string StreetName { get; private set; } = default!;
	public int StreetNumber { get; private set; } = default!;
	public string City { get; private set; } = default!;
	public string State { get; private set; } = default!;
	public string PostalCode { get; private set; } = default!;
	public string Country { get; private set; } = default!;

	public Address() { }

	public Address(string StreetName, int StreetNumber, string City, string State, string PostalCode, string Country)
	{
		this.StreetName = StreetName;
		this.StreetNumber = StreetNumber;
		this.City = City;
		this.State = State;
		this.PostalCode = PostalCode;
		this.Country = Country;
	}
}