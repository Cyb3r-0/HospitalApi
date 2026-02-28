using HospitalApi.Dtos;

public interface IPatientService
{
    Task<object> GetAllAsync(PatientQueryDto query);
    Task<PatientDto?> GetByIdAsync(int id);
    Task<PatientDto> CreateAsync(PatientCreateDto dto);
    Task<bool> UpdateAsync(int id, PatientUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}