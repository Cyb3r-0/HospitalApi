using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalApi.Services
{
    public class DoctorService : IDoctorService
    {
        private const int CacheDurationMinutes = 5;
        private readonly IDoctorRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public DoctorService(IDoctorRepository repo, IMapper mapper, IDistributedCache cache)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<PagedResult<DoctorDto>> GetAllAsync(DoctorQueryDto query)
        {
            if (query.Page <= 0) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var specKey = string.IsNullOrWhiteSpace(query.Specialization) ? "all" : query.Specialization;
            var availKey = query.IsAvailable.HasValue ? query.IsAvailable.Value.ToString() : "all";
            var cacheKey = $"doctors_page{query.Page}_size{query.PageSize}_spec_{specKey}_avail_{availKey}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    return JsonSerializer.Deserialize<PagedResult<DoctorDto>>(cachedData)!;
                }
                catch
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }

            var totalCount = await _repo.GetCountAsync(query.Specialization, query.IsAvailable);
            var doctors = await _repo.GetAllAsync(query.Page, query.PageSize, query.Specialization, query.IsAvailable);
            var mapped = _mapper.Map<List<DoctorDto>>(doctors);

            var result = new PagedResult<DoctorDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            return result;
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"doctor_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    return JsonSerializer.Deserialize<DoctorDto>(cachedData);
                }
                catch
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }

            var doctor = await _repo.GetByIdAsync(id);
            if (doctor == null) return null;

            var mapped = _mapper.Map<DoctorDto>(doctor);

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(mapped),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
                });

            return mapped;
        }

        public async Task<DoctorDto> CreateAsync(DoctorCreateDto dto, int createdByUserId)
        {
            var doctor = _mapper.Map<Doctor>(dto);
            doctor.CreatedByUserId = createdByUserId;

            await _repo.AddAsync(doctor);
            await _repo.SaveChangesAsync();

            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<bool> UpdateAsync(int id, DoctorUpdateDto dto)
        {
            var doctor = await _repo.GetByIdAsync(id);
            if (doctor == null) return false;

            _mapper.Map(dto, doctor);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync($"doctor_{id}");

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _repo.GetByIdAsync(id);
            if (doctor == null) return false;

            _repo.Delete(doctor);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync($"doctor_{id}");

            return true;
        }
    }
}
