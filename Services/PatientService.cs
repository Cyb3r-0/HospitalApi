using AutoMapper;
using HospitalApi.Dtos;
using HospitalApi.Models;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly IMapper _mapper;

    public PatientService(IPatientRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<object> GetAllAsync(PatientQueryDto query)
    {
        query.Page = query.Page <= 0 ? 1 : query.Page;
        query.PageSize = query.PageSize <= 0 ? 100 : query.PageSize;

        var totalCount = await _repo.GetCountAsync(query.Disease);

        var patients = await _repo.GetAllAsync(query.Page, query.PageSize, query.Disease);

        var data = _mapper.Map<List<PatientDto>>(patients);

        return new
        {
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPage = (int)Math.Ceiling(totalCount / (double)query.PageSize),
            Data = data
        };
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        return patient == null ? null : _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto> CreateAsync(PatientCreateDto dto)
    {
        var patient = _mapper.Map<Patient>(dto);

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

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        if (patient == null) return false;

        await _repo.DeleteAsync(patient);
        await _repo.SaveChangesAsync();

        return true;
    }
}