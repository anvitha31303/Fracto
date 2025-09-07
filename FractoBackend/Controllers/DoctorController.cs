using Fracto.Data;
using Fracto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DoctorController(AppDbContext db) => _db = db;

       [HttpGet]
public async Task<IActionResult> GetDoctors([FromQuery] string? name,[FromQuery] string? city, [FromQuery] int? specializationId, [FromQuery] double? minRating)
{
    var query = _db.Doctors.Include(d => d.Specialization).AsQueryable();
 if (!string.IsNullOrEmpty(name))
        query = query.Where(d => d.Name.Contains(name));
    // 🔎 Apply filters only if they are provided
    if (!string.IsNullOrWhiteSpace(city))
        query = query.Where(d => d.City.ToLower().Contains(city.ToLower()));

    if (specializationId.HasValue && specializationId.Value > 0)
        query = query.Where(d => d.SpecializationId == specializationId.Value);

    if (minRating.HasValue && minRating.Value > 0)
        query = query.Where(d => d.Rating >= minRating.Value);

    var doctors = await query
        .Select(d => new
        {
            d.DoctorId,
            d.Name,
            d.City,
            d.Rating,
            Specialization = new
            {
                d.Specialization.SpecializationId,
                d.Specialization.SpecializationName
            }
        })
        .ToListAsync();

    return Ok(doctors);
}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _db.Doctors.Include(d => d.Specialization)
                                          .FirstOrDefaultAsync(d => d.DoctorId == id);

            return doctor == null ? NotFound() : Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDoctor([FromBody] Doctor doctor)
        {
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();
            return Ok(doctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor updated)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.Name = updated.Name;
            doctor.City = updated.City;
            doctor.SpecializationId = updated.SpecializationId;
            doctor.Rating = updated.Rating;

            await _db.SaveChangesAsync();
            return Ok(doctor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Doctor deleted" });
        }
    }
}
