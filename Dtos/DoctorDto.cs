namespace HospitalApi.Dtos
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
