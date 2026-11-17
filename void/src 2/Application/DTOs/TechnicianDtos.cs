using Domain.Enums;

namespace Application.Dtos;

public sealed record TechnicianDto(
	Guid Id,
	Guid PropertyId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	decimal HourlyRate,
	IReadOnlyCollection<TechnicianCapability> Capabilities,
	DateTime CreatedAt
);

public sealed record CreateTechnicianRequest(
	Guid PropertyId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	string HashedPassword,
	decimal HourlyRate,
	IReadOnlyCollection<TechnicianCapability> Capabilities
);

public sealed record UpdateTechnicianRequest(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	decimal HourlyRate,
	IReadOnlyCollection<TechnicianCapability> Capabilities
);