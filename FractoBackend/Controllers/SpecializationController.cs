using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Fracto.Data;
using Fracto.Models;

[ApiController]
[Route("api/[controller]")]
public class SpecializationController : ControllerBase
{
    private readonly AppDbContext _db;
    public SpecializationController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Specializations.ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] Specialization s)
    {
        _db.Specializations.Add(s);
        await _db.SaveChangesAsync();
        return Ok(s);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpecialization(int id)
    {
        var spec = await _db.Specializations.FindAsync(id);
        if (spec == null) return NotFound();

        // Optional: check if any doctors are using this specialization
        var hasDoctors = await _db.Doctors.AnyAsync(d => d.SpecializationId == id);
        if (hasDoctors)
            return BadRequest("Cannot delete specialization: it is assigned to doctors.");

        _db.Specializations.Remove(spec);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Specialization deleted successfully" });
    }
}
