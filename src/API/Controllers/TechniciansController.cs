using Application.Interfaces;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/technicians")]
public sealed class TechniciansController : ControllerBase
{
	private readonly ITechnicianService _service;

	public TechniciansController(ITechnicianService service)
	{
		_service = service;
	}

	[HttpGet]
	public Task<List<TechnicianDto>> GetAll(CancellationToken ct)
	{
		return _service.GetAllAsync(ct);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<TechnicianDto>> GetById(Guid id, CancellationToken ct)
	{
		return (await _service.GetByIdAsync(id, ct)) is { } dto ?
			Ok(dto) :
			NotFound();
	}

	[HttpPost]
	public async Task<ActionResult<Guid>> Create([FromBody]CreateTechnicianRequest request, CancellationToken ct)
	{
		var id = await _service.CreateAsync(request, ct);

		return CreatedAtAction(nameof(GetById), new { id }, id);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, [FromBody]UpdateTechnicianRequest request, CancellationToken ct)
	{
		return await _service.UpdateAsync(id, request, ct) ?
			NoContent() :
			NotFound();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		return await _service.DeleteAsync(id, ct) ?
			NoContent() :
			NotFound();
	}
}