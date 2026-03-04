using HospitalApi.Models;

namespace HospitalApi.Repositories
{
    public interface IDoctorRepository
    {
        Task<List<Doctor>> GetAllAsync(int page, int pageSize, string? specialization, bool? isAvailable);
        Task<int> GetCountAsync(string? specialization, bool? isAvailable);
        Task<Doctor?> GetByIdAsync(int id);
        Task AddAsync(Doctor doctor);
        void Delete(Doctor doctor);
        Task SaveChangesAsync();
    }
}
