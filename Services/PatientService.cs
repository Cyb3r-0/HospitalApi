using System.Text.Json;
using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace HospitalApi.Services
{
    public class PatientService : IPatientService
    {
        private const int CacheDurationMinutes = 5;
        private readonly IPatientRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public PatientService(IPatientRepository repo, IMapper mapper, IDistributedCache cache)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<PagedResult<PatientDto>> GetAllAsync(PatientQueryDto query)
        {
            if (query.Page <= 0) query.Page = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            var diseaseKey = string.IsNullOrWhiteSpace(query.Disease) ? "all" : query.Disease;
            var cacheKey = $"patients_page{query.Page}_size{query.PageSize}_disease_{diseaseKey}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                try
                {
                    return JsonSerializer.Deserialize<PagedResult<PatientDto>>(cachedData)!;
                }
                catch
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }

            var totalCount = await _repo.GetCountAsync(query.Disease);
            var patients = await _repo.GetAllAsync(query.Page, query.PageSize, query.Disease);
            var mapped = _mapper.Map<List<PatientDto>>(patients);

            var result = new PagedResult<PatientDto>
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

        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var cacheKey = $"patient_{id}";
            var cacheData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cacheData))
            {
                try
                {
                    return JsonSerializer.Deserialize<PatientDto>(cacheData);
                }
                catch
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }

            var patient = await _repo.GetByIdAsync(id);
            if (patient == null)
                return null;

            var mapped = _mapper.Map<PatientDto>(patient);

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(mapped),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
                });

            return mapped;
        }

        public async Task<PatientDto> CreateAsync(PatientCreateDto dto, int createdByUserId)
        {
            var patient = _mapper.Map<Patient>(dto);
            patient.CreatedByUserId = createdByUserId;

            await _repo.AddAsync(patient);
            await _repo.SaveChangesAsync();

            return _mapper.Map<PatientDto>(patient);
        }

        public async Task<bool> UpdateAsync(int id, PatientUpdateDto dto)
        {
            var patient = await _repo.GetByIdAsync(id);
            if (patient == null) return false;

            _mapper.Map(dto, patient);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync($"patient_{id}");

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _repo.GetByIdAsync(id);
            if (patient == null) return false;

            _repo.Delete(patient);
            await _repo.SaveChangesAsync();
            await _cache.RemoveAsync($"patient_{id}");

            return true;
        }
    }
}