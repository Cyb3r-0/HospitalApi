using HospitalApi.Data;
using HospitalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _db;

        public DoctorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Doctor>> GetAllAsync(int page, int pageSize, string? specialization, bool? isAvailable)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _db.Doctors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(d => d.Specialization.Contains(specialization));

            if (isAvailable.HasValue)
                query = query.Where(d => d.IsAvailable == isAvailable.Value);

            return await query
                .OrderBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? specialization, bool? isAvailable)
        {
            var query = _db.Doctors.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(d => d.Specialization.Contains(specialization));

            if (isAvailable.HasValue)
                query = query.Where(d => d.IsAvailable == isAvailable.Value);

            return await query.CountAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
            => await _db.Doctors.FindAsync(id);

        public async Task AddAsync(Doctor doctor)
            => await _db.Doctors.AddAsync(doctor);

        public void Delete(Doctor doctor)
            => _db.Doctors.Remove(doctor);

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}
