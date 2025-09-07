using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fracto.Models
{
    public class Specialization
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpecializationId { get; set; }

        [Required]
        public string SpecializationName { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
