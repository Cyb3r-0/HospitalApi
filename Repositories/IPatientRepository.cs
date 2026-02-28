using HospitalApi.Models;

public interface IPatientRepository
{
    Task<List<Patient>> GetAllAsync(
        int page,
        int pageSize,
        string? disease);

    Task<int> GetCountAsync(string? disease);
    Task<Patient?> GetByIdAsync(int id);
    Task AddAsync(Patient patient);
    Task DeleteAsync(Patient patient);
    Task SaveChangesAsync();
}