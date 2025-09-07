using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fracto.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Range(1, 5)]
        public int RatingValue { get; set; } // ⭐ from 1 to 5

        public string? Comment { get; set; }

        public Doctor Doctor { get; set; }
        public User User { get; set; }
    }
}
