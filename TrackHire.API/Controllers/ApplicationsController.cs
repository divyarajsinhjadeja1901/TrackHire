using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrackHire.API.DTOs;
using TrackHire.API.Services;

namespace TrackHire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // ALL endpoints require JWT token
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    // Helper: Extract userId from JWT token
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/applications?status=Applied
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var apps = await _service.GetAllAsync(GetUserId(), status);
        return Ok(apps);
    }

    // GET api/applications/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var app = await _service.GetByIdAsync(id, GetUserId());
        if (app == null) return NotFound();
        return Ok(app);
    }

    // POST api/applications
    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationDto dto)
    {
        var created = await _service.CreateAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PATCH api/applications/5/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var result = await _service.UpdateStatusAsync(id, GetUserId(), newStatus);
        if (!result) return NotFound();
        return NoContent();
    }

    // DELETE api/applications/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // POST api/applications/5/notes
    [HttpPost("{id}/notes")]
    public async Task<IActionResult> AddNote(int id, [FromBody] string content)
    {
        await _service.AddNoteAsync(id, GetUserId(), content);
        return Ok();
    }
}