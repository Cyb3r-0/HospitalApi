using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _db;

        public AppointmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Appointment>> GetAllAsync(AppointmentQueryDto query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var q = _db.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => !a.IsDeleted)
                .AsQueryable();

            if (query.DoctorId.HasValue)
                q = q.Where(a => a.DoctorId == query.DoctorId.Value);

            if (query.PatientId.HasValue)
                q = q.Where(a => a.PatientId == query.PatientId.Value);

            if (query.Status.HasValue)
                q = q.Where(a => a.Status == query.Status.Value);

            if (query.From.HasValue)
                q = q.Where(a => a.AppointmentDate >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(a => a.AppointmentDate <= query.To.Value);

            return await q
                .OrderBy(a => a.AppointmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(AppointmentQueryDto query)
        {
            var q = _db.Appointments.AsNoTracking().Where(a => !a.IsDeleted).AsQueryable();

            if (query.DoctorId.HasValue)
                q = q.Where(a => a.DoctorId == query.DoctorId.Value);

            if (query.PatientId.HasValue)
                q = q.Where(a => a.PatientId == query.PatientId.Value);

            if (query.Status.HasValue)
                q = q.Where(a => a.Status == query.Status.Value);

            if (query.From.HasValue)
                q = q.Where(a => a.AppointmentDate >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(a => a.AppointmentDate <= query.To.Value);

            return await q.CountAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
            => await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        public async Task<bool> DoctorExistsAsync(int doctorId)
            => await _db.Doctors.AnyAsync(d => d.Id == doctorId);

        public async Task<bool> PatientExistsAsync(int patientId)
            => await _db.Patients.AnyAsync(p => p.Id == patientId);

        public async Task AddAsync(Appointment appointment)
            => await _db.Appointments.AddAsync(appointment);

        //public void Delete(Appointment appointment)
        //    => _db.Appointments.Remove(appointment);

        public void Delete(Appointment appointment)
        {
            appointment.IsDeleted = true;
            _db.Appointments.Update(appointment);
        }

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}