using Application.Interfaces;
using Application.Dtos;
using Domain.Abstractions;
using Domain.Entities.Users;

namespace Application.Services;

public sealed class ManagerService : IManagerService
{
	private readonly IUserReadRepository _reader;
	private readonly IUserWriteRepository _writer;
	private readonly IPropertyLookup _properties;

	public ManagerService(IUserReadRepository reader, IUserWriteRepository writer, IPropertyLookup properties)
	{
		_reader = reader;
		_writer = writer;
		_properties = properties;
	}

	public async Task<List<ManagerDto>> GetAllAsync(CancellationToken ct)
	{
		return (await _reader.GetManagersAsync(ct))
			.Select(m => new ManagerDto(
				m.Id,
				m.Property.Id,
				m.FirstName,
				m.LastName,
				m.Email,
				m.PhoneNumber,
				m.HourlyRate,
				m.CreatedAt
			))
			.ToList();
	}

	public async Task<ManagerDto?> GetByIdAsync(Guid id, CancellationToken ct)
	{
		var m = await _reader.GetManagerByIdAsync(id, ct);

		return m is null ? null : new ManagerDto(
			m.Id,
			m.Property.Id,
			m.FirstName,
			m.LastName,
			m.Email,
			m.PhoneNumber,
			m.HourlyRate,
			m.CreatedAt
		);
	}

	public async Task<Guid> CreateAsync(CreateManagerRequest request, CancellationToken ct)
	{
		var property = await _properties.GetByIdAsync(request.PropertyId, ct) ??
			throw new InvalidOperationException("Property not found.");

		// Manager constructor sets Role internally.
		var manager = new Manager
		{
			Property = property,
			HourlyRate = request.HourlyRate,
			FirstName = request.FirstName,
			LastName = request.LastName,
			Email = request.Email,
			PhoneNumber = request.PhoneNumber,
			HashedPassword = request.HashedPassword
		};

		await _writer.AddAsync(manager, ct);
		await _writer.SaveChangesAsync(ct);
		
		return manager.Id;
	}

	public async Task<bool> UpdateAsync(Guid id, UpdateManagerRequest request, CancellationToken ct)
	{
		var m = await _reader.GetManagerByIdAsync(id, ct);
		if (m is null)
			return false;

		m.FirstName = request.FirstName;
		m.LastName = request.LastName;
		m.Email = request.Email;
		m.PhoneNumber = request.PhoneNumber;
		m.HourlyRate = request.HourlyRate;

		await _writer.SaveChangesAsync(ct);
		
		return true;
	}

	public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
	{
		var m = await _reader.GetManagerByIdAsync(id, ct);
		if (m is null)
			return false;

		await _writer.DeleteAsync(m, ct);
		await _writer.SaveChangesAsync(ct);

		return true;
	}
}