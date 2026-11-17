using Application.Interfaces;
using Application.Dtos;
using Domain.Abstractions;
using Domain.Entities.Users;

namespace Application.Services;

public sealed class TenantService : ITenantService
{
	private readonly IUserReadRepository _reader;
	private readonly IUserWriteRepository _writer;
	private readonly IUnitLookup _units;

	public TenantService(IUserReadRepository reader, IUserWriteRepository writer, IUnitLookup units)
	{
		_reader = reader;
		_writer = writer;
		_units = units;
	}

	public async Task<List<TenantDto>> GetAllAsync(CancellationToken ct)
	{
		return (await _reader.GetTenantsAsync(ct))
			.Select(t => new TenantDto(
				t.Id,
				t.Unit.Id,
				t.FirstName,
				t.LastName,
				t.Email,
				t.PhoneNumber,
				t.MovedIn,
				t.MovedOut,
				t.CreatedAt
			))
			.ToList();
	}

	public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken ct)
	{
		var t = await _reader.GetTenantByIdAsync(id, ct);

		return t is null ? null : new TenantDto(
			t.Id,
			t.Unit.Id,
			t.FirstName,
			t.LastName,
			t.Email,
			t.PhoneNumber,
			t.MovedIn,
			t.MovedOut,
			t.CreatedAt
		);
	}

	public async Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken ct)
	{
		var unit = await _units.GetByIdAsync(request.UnitId, ct) ??
			throw new InvalidOperationException("Unit not found.");

		if (await _units.IsUnitOccupiedAsync(request.UnitId, ct))
			throw new InvalidOperationException("Unit is already occupied.");

		// Tenant constructor sets Role internally.
		var tenant = new Tenant
		{
			Unit = unit,
			MovedIn = request.MovedIn,
			MovedOut = request.MovedOut,
			FirstName = request.FirstName,
			LastName = request.LastName,
			Email = request.Email,
			PhoneNumber = request.PhoneNumber,
			HashedPassword = request.HashedPassword
		};

		// Keep object graph consistent
		// (Unit has Tenant navigation)
		unit.Tenant = tenant;

		await _writer.AddAsync(tenant, ct);
		await _writer.SaveChangesAsync(ct);

		return tenant.Id;
	}

	public async Task<bool> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct)
	{
		var t = await _reader.GetTenantByIdAsync(id, ct);
		if (t is null)
			return false;

		t.FirstName = request.FirstName;
		t.LastName = request.LastName;
		t.Email = request.Email;
		t.PhoneNumber = request.PhoneNumber;
		t.MovedIn = request.MovedIn;
		t.MovedOut = request.MovedOut;

		await _writer.SaveChangesAsync(ct);

		return true;
	}

	public async Task<bool> AssignUnitAsync(Guid id, AssignTenantUnitRequest request, CancellationToken ct)
	{
		var t = await _reader.GetTenantByIdAsync(id, ct);
		if (t is null)
			return false;

		var u = await _units.GetByIdAsync(request.UnitId, ct);
		if (u is null)
			throw new InvalidOperationException("Unit not found.");

		if (await _units.IsUnitOccupiedAsync(request.UnitId, ct))
			throw new InvalidOperationException("Unit is already occupied.");

		t.Unit = u;
		u.Tenant = t;

		await _writer.SaveChangesAsync(ct);

		return true;
	}


	public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
	{
		var t = await _reader.GetTenantByIdAsync(id, ct);
		if (t is null)
			return false;

		// Clear occupancy
		t.Unit.Tenant = null!;

		await _writer.DeleteAsync(t, ct);
		await _writer.SaveChangesAsync(ct);

		return true;
	}
}