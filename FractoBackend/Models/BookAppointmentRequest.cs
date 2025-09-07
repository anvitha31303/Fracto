namespace Fracto.Models.DTOs
{
    public class BookAppointmentRequest
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
    }
}
