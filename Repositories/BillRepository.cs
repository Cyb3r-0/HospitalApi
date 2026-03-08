using HospitalApi.Data;
using HospitalApi.Dtos;
using HospitalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Repositories
{
    public class BillRepository : IBillRepository
    {
        private readonly AppDbContext _db;

        public BillRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Bill>> GetAllAsync(BillQueryDto query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var q = _db.Bills
                .AsNoTracking()
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            if (query.PatientId.HasValue)
                q = q.Where(b => b.PatientId == query.PatientId.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(b => b.DoctorId == query.DoctorId.Value);

            if (query.AppointmentId.HasValue)
                q = q.Where(b => b.AppointmentId == query.AppointmentId.Value);

            if (query.PaymentStatus.HasValue)
                q = q.Where(b => b.PaymentStatus == query.PaymentStatus.Value);

            if (query.From.HasValue)
                q = q.Where(b => b.CreatedAt >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(b => b.CreatedAt <= query.To.Value);

            return await q
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(BillQueryDto query)
        {
            var q = _db.Bills.AsNoTracking().Where(b => !b.IsDeleted).AsQueryable();

            if (query.PatientId.HasValue)
                q = q.Where(b => b.PatientId == query.PatientId.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(b => b.DoctorId == query.DoctorId.Value);

            if (query.AppointmentId.HasValue)
                q = q.Where(b => b.AppointmentId == query.AppointmentId.Value);

            if (query.PaymentStatus.HasValue)
                q = q.Where(b => b.PaymentStatus == query.PaymentStatus.Value);

            if (query.From.HasValue)
                q = q.Where(b => b.CreatedAt >= query.From.Value);

            if (query.To.HasValue)
                q = q.Where(b => b.CreatedAt <= query.To.Value);

            return await q.CountAsync();
        }

        public async Task<Bill?> GetByIdAsync(int id)
            => await _db.Bills
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        public async Task<bool> AppointmentExistsAsync(int appointmentId)
            => await _db.Appointments.AnyAsync(a => a.Id == appointmentId && !a.IsDeleted);

        public async Task<bool> BillExistsForAppointmentAsync(int appointmentId)
            => await _db.Bills.AnyAsync(b => b.AppointmentId == appointmentId && !b.IsDeleted);

        public async Task<Appointment?> GetAppointmentAsync(int appointmentId)
            => await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && !a.IsDeleted);

        public async Task AddAsync(Bill bill)
            => await _db.Bills.AddAsync(bill);

        public void Delete(Bill bill)
        {
            bill.IsDeleted = true;
            bill.DeletedAt = DateTime.UtcNow;
            _db.Bills.Update(bill);
        }

        public async Task SaveChangesAsync()
            => await _db.SaveChangesAsync();
    }
}