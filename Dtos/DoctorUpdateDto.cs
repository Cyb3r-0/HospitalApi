namespace HospitalApi.Dtos
{
    public class DoctorUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsAvailable { get; set; }
    }
}
