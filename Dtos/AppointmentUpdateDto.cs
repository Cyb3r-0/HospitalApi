using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class AppointmentUpdateDto
    {
        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
