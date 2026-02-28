using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, 120)]
        public int Age { get; set; }

        [StringLength(200)]
        public string? Disease { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
    }
}
