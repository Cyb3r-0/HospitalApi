namespace HospitalApi.Dtos
{
    public class AppointmentCreateDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
    }
}
