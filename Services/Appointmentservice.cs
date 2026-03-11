using System.Text.Json;
using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace HospitalApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private const int CacheDurationMinutes = 5;
        private readonly IAppointmentRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public AppointmentService(IAppointmentRepository repo, IMapper mapper, IDistributedCache cache)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<PagedResult<AppointmentDto>> GetAllAsync(AppointmentQueryDto query)
        {
            if (query.Page <= 0) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var cacheKey = $"appointments_p{query.Page}_s{query.PageSize}" +
                           $"_doc{query.DoctorId}_pat{query.PatientId}" +
                           $"_status{query.Status}_from{query.From}_to{query.To}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    try { return JsonSerializer.Deserialize<PagedResult<AppointmentDto>>(cachedData)!; }
                    catch { await _cache.RemoveAsync(cacheKey); }
                }
            }
            catch { /* Redis unavailable — continue to DB */ }

            var totalCount = await _repo.GetCountAsync(query);
            var appointments = await _repo.GetAllAsync(query);
            var mapped = _mapper.Map<List<AppointmentDto>>(appointments);

            var result = new PagedResult<AppointmentDto>
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
            catch { /* Redis unavailable — continue without caching */ }

            return result;
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"appointment_{id}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    try { return JsonSerializer.Deserialize<AppointmentDto>(cachedData); }
                    catch { await _cache.RemoveAsync(cacheKey); }
                }
            }
            catch { /* Redis unavailable — continue to DB */ }

            var appointment = await _repo.GetByIdAsync(id);
            if (appointment == null) return null;

            var mapped = _mapper.Map<AppointmentDto>(appointment);

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
            catch { /* Redis unavailable — continue without caching */ }

            return mapped;
        }

        public async Task<(AppointmentDto? Result, string? Error)> CreateAsync(AppointmentCreateDto dto, int createdByUserId)
        {
            // Validate doctor exists
            if (!await _repo.DoctorExistsAsync(dto.DoctorId))
                return (null, $"Doctor with ID {dto.DoctorId} does not exist.");

            // Validate patient exists
            if (!await _repo.PatientExistsAsync(dto.PatientId))
                return (null, $"Patient with ID {dto.PatientId} does not exist.");

            var appointment = _mapper.Map<Appointment>(dto);
            appointment.CreatedByUserId = createdByUserId;
            appointment.Status = AppointmentStatus.Scheduled;

            await _repo.AddAsync(appointment);
            await _repo.SaveChangesAsync();

            // Reload with Patient + Doctor navigation properties for response
            var created = await _repo.GetByIdAsync(appointment.Id);
            return (_mapper.Map<AppointmentDto>(created), null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, AppointmentUpdateDto dto)
        {
            var appointment = await _repo.GetByIdAsync(id);
            if (appointment == null) return (false, null);

            // Cannot reschedule a cancelled appointment
            if (appointment.Status == AppointmentStatus.Cancelled)
                return (false, "Cannot update a cancelled appointment.");

            _mapper.Map(dto, appointment);
            await _repo.SaveChangesAsync();

            try { await _cache.RemoveAsync($"appointment_{id}"); }
            catch { /* Redis unavailable */ }

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _repo.GetByIdAsync(id);
            if (appointment == null) return false;

            _repo.Delete(appointment);
            await _repo.SaveChangesAsync();

            try { await _cache.RemoveAsync($"appointment_{id}"); }
            catch { /* Redis unavailable */ }

            return true;
        }
    }
}