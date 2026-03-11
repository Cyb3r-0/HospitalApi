using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Required, StringLength(100)]
        public string Specialization { get; set; } = null!;

        [StringLength(15)]
        public string? Phone { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
    }
}
