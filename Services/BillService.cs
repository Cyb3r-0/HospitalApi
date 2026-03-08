using System.Text.Json;
using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace HospitalApi.Services
{
    public class BillService : IBillService
    {
        private const int CacheDurationMinutes = 5;
        private readonly IBillRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public BillService(IBillRepository repo, IMapper mapper, IDistributedCache cache)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<PagedResult<BillDto>> GetAllAsync(BillQueryDto query)
        {
            if (query.Page <= 0) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var cacheKey = $"bills_p{query.Page}_s{query.PageSize}" +
                           $"_pat{query.PatientId}_doc{query.DoctorId}" +
                           $"_status{query.PaymentStatus}_from{query.From}_to{query.To}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    try { return JsonSerializer.Deserialize<PagedResult<BillDto>>(cachedData)!; }
                    catch { await _cache.RemoveAsync(cacheKey); }
                }
            }
            catch { /* Redis unavailable — continue to DB */ }

            var totalCount = await _repo.GetCountAsync(query);
            var bills = await _repo.GetAllAsync(query);
            var mapped = _mapper.Map<List<BillDto>>(bills);

            var result = new PagedResult<BillDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(result),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                    });
            }
            catch { /* Redis unavailable */ }

            return result;
        }

        public async Task<BillDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"bill_{id}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    try { return JsonSerializer.Deserialize<BillDto>(cachedData); }
                    catch { await _cache.RemoveAsync(cacheKey); }
                }
            }
            catch { /* Redis unavailable */ }

            var bill = await _repo.GetByIdAsync(id);
            if (bill == null) return null;

            var mapped = _mapper.Map<BillDto>(bill);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
                    });
            }
            catch { /* Redis unavailable */ }

            return mapped;
        }

        public async Task<(BillDto? Result, string? Error)> CreateAsync(BillCreateDto dto, int createdByUserId)
        {
            // Validate appointment exists
            if (!await _repo.AppointmentExistsAsync(dto.AppointmentId))
                return (null, $"Appointment with ID {dto.AppointmentId} does not exist.");

            // Prevent duplicate bill for same appointment
            if (await _repo.BillExistsForAppointmentAsync(dto.AppointmentId))
                return (null, $"A bill already exists for Appointment ID {dto.AppointmentId}.");

            // Load appointment to get PatientId and DoctorId
            var appointment = await _repo.GetAppointmentAsync(dto.AppointmentId);

            var bill = _mapper.Map<Bill>(dto);
            bill.PatientId = appointment!.PatientId;
            bill.DoctorId = appointment.DoctorId;
            bill.CreatedByUserId = createdByUserId;
            bill.InvoiceNumber = GenerateInvoiceNumber();

            // Auto-calculate total: (consultation + medicine + other + tax) - discount
            bill.TotalAmount = (dto.ConsultationFee + dto.MedicineFee + dto.OtherCharges + dto.TaxAmount) - dto.Discount;

            await _repo.AddAsync(bill);
            await _repo.SaveChangesAsync();

            var created = await _repo.GetByIdAsync(bill.Id);
            return (_mapper.Map<BillDto>(created), null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, BillUpdateDto dto, int updatedByUserId)
        {
            var bill = await _repo.GetByIdAsync(id);
            if (bill == null) return (false, null);

            // Cannot edit a paid bill
            if (bill.PaymentStatus == PaymentStatus.Paid)
                return (false, "Cannot update a fully paid bill.");

            _mapper.Map(dto, bill);
            bill.TotalAmount = (dto.ConsultationFee + dto.MedicineFee + dto.OtherCharges + dto.TaxAmount) - dto.Discount;
            bill.UpdatedAt = DateTime.UtcNow;
            bill.UpdatedByUserId = updatedByUserId;

            await _repo.SaveChangesAsync();

            try { await _cache.RemoveAsync($"bill_{id}"); }
            catch { /* Redis unavailable */ }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> PayAsync(int id, BillPayDto dto, int updatedByUserId)
        {
            var bill = await _repo.GetByIdAsync(id);
            if (bill == null) return (false, null);

            if (bill.PaymentStatus == PaymentStatus.Cancelled)
                return (false, "Cannot pay a cancelled bill.");

            if (bill.PaymentStatus == PaymentStatus.Paid)
                return (false, "Bill is already fully paid.");

            bill.PaidAmount += dto.PaidAmount;
            bill.PaymentMethod = dto.PaymentMethod;
            bill.TransactionReference = dto.TransactionReference;
            bill.UpdatedAt = DateTime.UtcNow;
            bill.UpdatedByUserId = updatedByUserId;

            if (bill.PaidAmount >= bill.TotalAmount)
            {
                bill.PaymentStatus = PaymentStatus.Paid;
                bill.PaidAt = DateTime.UtcNow;
            }
            else
            {
                bill.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            await _repo.SaveChangesAsync();

            try { await _cache.RemoveAsync($"bill_{id}"); }
            catch { /* Redis unavailable */ }

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bill = await _repo.GetByIdAsync(id);
            if (bill == null) return false;

            _repo.Delete(bill);
            await _repo.SaveChangesAsync();

            try { await _cache.RemoveAsync($"bill_{id}"); }
            catch { /* Redis unavailable */ }

            return true;
        }

        // Generates invoice number e.g. INV-20260306-A3F2
        private static string GenerateInvoiceNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var unique = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"INV-{date}-{unique}";
        }
    }
}