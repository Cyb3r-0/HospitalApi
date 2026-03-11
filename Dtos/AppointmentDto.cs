using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class AppointmentDto
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string DoctorSpecialization { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
        public string StatusName => Status.ToString(); // e.g. "Scheduled"

        public DateTime CreatedAt { get; set; }
    }
}
