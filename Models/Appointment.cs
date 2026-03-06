using System.ComponentModel.DataAnnotations;

namespace HospitalApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
    }
}
