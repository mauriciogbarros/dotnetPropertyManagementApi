namespace Application.Dtos;

public sealed record ManagerDto(
	Guid Id,
	Guid PropertyId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	decimal HourlyRate,
	DateTime CreatedAt
);

public sealed record CreateManagerRequest(
	Guid PropertyId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	string HashedPassword,
	decimal HourlyRate
);

public sealed record UpdateManagerRequest(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	decimal HourlyRate
);
