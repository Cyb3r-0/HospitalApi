using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class AppointmentQueryDto
    {
        private const int MaxPageSize = 50;

        public int Page { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Filter by doctor e.g. ?doctorId=1
        public int? DoctorId { get; set; }

        // Filter by patient e.g. ?patientId=2
        public int? PatientId { get; set; }

        // Filter by status e.g. ?status=1 (Scheduled)
        public AppointmentStatus? Status { get; set; }

        // Filter by date range e.g. ?from=2025-01-01&to=2025-12-31
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
