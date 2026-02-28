using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Dtos
{
    public class PatientCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Disease { get; set; }
    }
}
