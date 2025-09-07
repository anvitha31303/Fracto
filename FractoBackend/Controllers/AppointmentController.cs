using Fracto.Data;
using Fracto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Fracto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AppointmentController(AppDbContext db)
        {
            _db = db;
        }

        // ✅ BOOK APPOINTMENT
        [HttpPost("book")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> BookAppointment([FromBody] Appointment request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get user id from JWT
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if doctor exists
            var doctor = await _db.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
                return BadRequest("Doctor not found");

            // Check time-slot conflict
            bool conflict = await _db.Appointments.AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.AppointmentDate.Date == request.AppointmentDate.Date &&
                a.TimeSlot == request.TimeSlot &&
                a.Status != "Cancelled");

            if (conflict)
                return BadRequest("This time slot is already booked.");

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                UserId = userId,
                AppointmentDate = request.AppointmentDate,
                TimeSlot = request.TimeSlot,
                Status = "Pending"
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Appointment booked successfully", appointment });
        }
        [HttpDelete("{id}/deleterecord")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteAppointment(int id)
{
    var appointment = await _db.Appointments.FindAsync(id);
    if (appointment == null)
        return NotFound("Appointment not found");

    _db.Appointments.Remove(appointment);
    await _db.SaveChangesAsync();

    return Ok(new { message = "Appointment deleted successfully" });
}

        // ✅ GET LOGGED-IN USER'S APPOINTMENTS
        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var appointments = await _db.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialization)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ CANCEL USER APPOINTMENT
        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == userId);

            if (appointment == null)
                return NotFound("Appointment not found");

            appointment.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Appointment cancelled" });
        }

        // ✅ ADMIN: GET ALL APPOINTMENTS
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _db.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Specialization)
                .Include(a => a.User)
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ ADMIN: UPDATE STATUS (Approve/Complete/Cancel)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            appointment.Status = status;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
    }
}
