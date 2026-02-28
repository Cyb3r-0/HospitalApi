using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Dtos
{
    public class PatientUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = null;

        [Range(0, 120)]
        public int age { get; set; }

        [StringLength(200)]
        public string? Disease { get; set; }
    }
}
