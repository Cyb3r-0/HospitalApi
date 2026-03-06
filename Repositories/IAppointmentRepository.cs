using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Repositories
{
    public interface IAppointmentRepository
    {
        Task<List<Appointment>> GetAllAsync(AppointmentQueryDto query);
        Task<int> GetCountAsync(AppointmentQueryDto query);
        Task<Appointment?> GetByIdAsync(int id);
        Task<bool> DoctorExistsAsync(int doctorId);
        Task<bool> PatientExistsAsync(int patientId);
        Task AddAsync(Appointment appointment);
        void Delete(Appointment appointment);
        Task SaveChangesAsync();
    }
}
