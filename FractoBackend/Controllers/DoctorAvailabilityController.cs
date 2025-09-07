using Microsoft.AspNetCore.Mvc;
using Fracto.Data;
using System;
using System.Linq;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext _db;
    public BookingController(AppDbContext db) => _db = db;

    // GET: api/Booking/doctor/{doctorId}/available-slots
    [HttpGet("doctor/{doctorId}/available-slots")]
    public IActionResult GetAvailableSlots(int doctorId)
    {
        // 1️⃣ Get all booked slots from appointments
        var bookedSlots = _db.Appointments
                             .Where(a => a.DoctorId == doctorId)
                             .Select(a => a.TimeSlot)
                             .ToList();

        // 2️⃣ Define working hours and slot duration
        var startTime = TimeSpan.FromHours(9); // 9:00 AM
        var endTime = TimeSpan.FromHours(17);  // 5:00 PM
        var slotDuration = TimeSpan.FromMinutes(30);

        var slots = new List<string>();

        // 3️⃣ Generate all possible slots
        while (startTime < endTime)
        {
            var slot = $"{startTime:hh\\:mm}-{(startTime + slotDuration):hh\\:mm}";

            // 4️⃣ Exclude booked slots
            if (!bookedSlots.Contains(slot))
                slots.Add(slot);

            startTime += slotDuration;
        }

        return Ok(slots);
    }
}
