    namespace HospitalApi.Dtos
{
    public class PatientDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string? Disease { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
