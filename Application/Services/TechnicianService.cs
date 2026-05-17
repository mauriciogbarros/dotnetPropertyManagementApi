using Application.Abstractions;
using Application.Dtos;
using Domain.Abstractions;
using Domain.Entities.Users;

namespace Application.Services;

public sealed class TechnicianService : ITechnicianService
{
	private readonly IUserReadRepository _reader;
	private readonly IUserWriteRepository _writer;
	private readonly IPropertyLookup _properties;

	public TechnicianService(IUserReadRepository reader, IUserWriteRepository writer, IPropertyLookup properties)
	{
		_reader = reader;
		_writer = writer;
		_properties = properties;
	}

	public async Task<List<TechnicianDto>> GetAllAsync(CancellationToken ct)
	{
		return (await _reader.GetTechniciansAsync(ct))
			.Select(t => new TechnicianDto(
				t.Id,
				t.Property.Id,
				t.FirstName,
				t.LastName,
				t.Email,
				t.PhoneNumber,
				t.HourlyRate,
				t.Capabilities.ToList(),
				t.CreatedAt
			))
			.ToList();
	}

	public async Task<TechnicianDto?> GetByIdAsync(Guid id, CancellationToken ct)
	{
		var t = await _reader.GetTechnicianByIdAsync(id, ct);
		return t is null ? null : new TechnicianDto(
			t.Id,
			t.Property.Id,
			t.FirstName,
			t.LastName,
			t.Email,
			t.PhoneNumber,
			t.HourlyRate,
			t.Capabilities.ToList(),
			t.CreatedAt
		);
	}

	public async Task<Guid> CreateAsync(CreateTechnicianRequest request, CancellationToken ct)
	{
		var property = await _properties.GetByIdAsync(request.PropertyId, ct) ??
			throw new InvalidOperationException("Property not found.");

		// Technician constructor sets Role internally.
		var tech = new Technician
		{
			Property = property,
			HourlyRate = request.HourlyRate,
			Capabilities = request.Capabilities.ToList(),
			FirstName = request.FirstName,
			LastName = request.LastName,
			Email = request.Email,
			PhoneNumber = request.PhoneNumber,
			HashedPassword = request.HashedPassword
		};

		await _writer.AddAsync(tech, ct);
		await _writer.SaveChangesAsync(ct);
		
		return tech.Id;
	}

	public async Task<bool> UpdateAsync(Guid id, UpdateTechnicianRequest request, CancellationToken ct)
	{
		var t = await _reader.GetTechnicianByIdAsync(id, ct);
		if (t is null)
			return false;

		t.FirstName = request.FirstName;
		t.LastName = request.LastName;
		t.Email = request.Email;
		t.PhoneNumber = request.PhoneNumber;
		t.HourlyRate = request.HourlyRate;
		t.Capabilities = request.Capabilities.Distinct().ToList();

		await _writer.SaveChangesAsync(ct);

		return true;
	}

	public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
	{
		var t = await _reader.GetTechnicianByIdAsync(id, ct);
		if (t is null)
			return false;

		await _writer.DeleteAsync(t, ct);
		await _writer.SaveChangesAsync(ct);
		
		return true;
	}
}