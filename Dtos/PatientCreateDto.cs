using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Dtos
{
    public class PatientCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = null;

        [Range(0, 120)]
        public int Age { get; set; }

        [StringLength(200)]
        public string? Disease { get; set; }
    }
}
