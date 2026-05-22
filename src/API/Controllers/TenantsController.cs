using Application.Interfaces;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _service;

    public TenantsController(ITenantService service)
		{
			_service = service;
		}

    [HttpGet]
    public Task<List<TenantDto>> GetAll(CancellationToken ct)
		{
      return _service.GetAllAsync(ct);
		}

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken ct)
		{
      return (await _service.GetByIdAsync(id, ct)) is { } dto ?
				Ok(dto) :
				NotFound();
		}

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var id = await _service.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken ct)
		{
      return await _service.UpdateAsync(id, request, ct) ?
				NoContent() :
				NotFound();
		}

    [HttpPut("{id:guid}/assign-unit")]
    public async Task<IActionResult> AssignUnit(Guid id, [FromBody] AssignTenantUnitRequest request, CancellationToken ct)
		{
      return await _service.AssignUnitAsync(id, request, ct) ?
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