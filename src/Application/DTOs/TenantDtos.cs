namespace Application.Dtos;

public sealed record TenantDto(
	Guid Id,
	Guid UnitId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	DateTime MovedIn,
	DateTime MovedOut,
	DateTime CreatedAt
);

public sealed record CreateTenantRequest(
	Guid UnitId,
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	string HashedPassword,
	DateTime MovedIn,
	DateTime MovedOut
);

public sealed record UpdateTenantRequest(
	string FirstName,
	string LastName,
	string Email,
	string PhoneNumber,
	DateTime MovedIn,
	DateTime MovedOut
);

public sealed record AssignTenantUnitRequest(Guid UnitId);