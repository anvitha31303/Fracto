using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fracto.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }  // remove [Required]

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }  // remove [Required]

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string TimeSlot { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}