using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/managers")]
public sealed class ManagersController : ControllerBase
{
	private readonly IManagerService _service;
	
	public ManagersController(IManagerService service)
	{
		_service = service;
	}

	[HttpGet]
	public Task<List<ManagerDto>> GetAll(CancellationToken ct)
	{
		return _service.GetAllAsync(ct);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<ManagerDto>> GetById(Guid id, CancellationToken ct)
	{
		return (await _service.GetByIdAsync(id, ct)) is { } dto ? 
			Ok(dto) :
			NotFound();
	}

	[HttpPost]
	public async Task<ActionResult<Guid>> CreatedAtActionResult([FromBody]CreateManagerRequest request, CancellationToken ct)
	{
		var id = await _service.CreateAsync(request, ct);
		return CreatedAtAction(nameof(GetById), new { id }, id);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, [FromBody]UpdateManagerRequest request, CancellationToken ct)
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