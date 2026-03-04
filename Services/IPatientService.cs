using System.Threading.Tasks;
using HospitalApi.Dtos;
using HospitalApi.Helpers;

public interface IPatientService
{
    Task<PagedResult<PatientDto>> GetAllAsync(PatientQueryDto query);
    Task<PatientDto?> GetByIdAsync(int id);
    Task<PatientDto> CreateAsync(PatientCreateDto dto, int id);
    Task<bool> UpdateAsync(int id, PatientUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}