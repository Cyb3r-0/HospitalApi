using HospitalApi.Dtos;
using HospitalApi.Models;

namespace HospitalApi.Repositories
{
    public interface IBillRepository
    {
        Task<List<Bill>> GetAllAsync(BillQueryDto query);
        Task<int> GetCountAsync(BillQueryDto query);
        Task<Bill?> GetByIdAsync(int id);
        Task<bool> AppointmentExistsAsync(int appointmentId);
        Task<bool> BillExistsForAppointmentAsync(int appointmentId);
        Task<Appointment?> GetAppointmentAsync(int appointmentId);
        Task AddAsync(Bill bill);
        void Delete(Bill bill);
        Task SaveChangesAsync();
    }
}