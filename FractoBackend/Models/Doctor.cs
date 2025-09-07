using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fracto.Models
{
    public class Doctor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DoctorId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Specialization")]
        public int SpecializationId { get; set; }

        public double Rating { get; set; } = 0;

        // Navigation property
        public Specialization? Specialization { get; set; }
    }
}
