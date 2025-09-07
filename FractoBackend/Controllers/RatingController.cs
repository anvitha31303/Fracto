using Fracto.Data;
using Fracto.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly AppDbContext _db;

    public RatingController(AppDbContext db)
    {
        _db = db;
    }

    // ✅ Get all ratings for a doctor
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorRatings(int doctorId)
    {
        var ratings = await _db.Ratings
            .Where(r => r.DoctorId == doctorId)
            .Include(r => r.User)
            .Select(r => new
            {
                r.RatingId,
                r.RatingValue,
                r.Comment,
                UserName = r.User.Username
            })
            .ToListAsync();

        var avgRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0;

        return Ok(new { averageRating = avgRating, ratings });
    }

    // ✅ Add rating (User must have completed appointment)
    [HttpPost]
[Authorize(Roles = "User")]
public async Task<IActionResult> AddRating([FromBody] RatingRequest req)
{
    bool hasCompletedAppointment = await _db.Appointments
        .AnyAsync(a => a.UserId == req.UserId &&
                       a.DoctorId == req.DoctorId &&
                       a.Status == "Completed");

    if (!hasCompletedAppointment)
        return BadRequest("You can only rate doctors after a completed appointment.");

    var rating = new Rating
    {
        DoctorId = req.DoctorId,
        UserId = req.UserId,
        RatingValue = req.RatingValue,
        Comment = req.Comment
    };

    _db.Ratings.Add(rating);
    await _db.SaveChangesAsync();

    // Update doctor's average rating
    var doctorRatings = await _db.Ratings.Where(r => r.DoctorId == req.DoctorId).ToListAsync();
    var newAvg = doctorRatings.Average(r => r.RatingValue);
    var doctor = await _db.Doctors.FindAsync(req.DoctorId);
    if (doctor != null)
    {
        doctor.Rating = newAvg;
        _db.Doctors.Update(doctor);
        await _db.SaveChangesAsync();
    }

    return Ok(new { message = "Rating added successfully", newAverage = newAvg });
}

}