using HospitalApi.Data;
using HospitalApi.Models;
using Microsoft.EntityFrameworkCore;
public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;

    public PatientRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Patient>> GetAllAsync(int page, int pageSize, string? disease)
    {
        var query = _db.Patients.AsNoTracking();

        if (!string.IsNullOrEmpty(disease))
            query = query.Where(p => p.Disease!.Contains(disease));

        return await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(string? disease)
    {
        var query = _db.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(disease))
            query = query.Where(p => p.Disease!.Contains(disease));

        return await query.CountAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id)
        => await _db.Patients.FindAsync(id);

    public async Task AddAsync(Patient patient)
        => await _db.Patients.AddAsync(patient);

    public async Task DeleteAsync(Patient patient)
        => _db.Patients.Remove(patient);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
