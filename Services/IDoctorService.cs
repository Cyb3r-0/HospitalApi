using HospitalApi.Dtos;
using HospitalApi.Helpers;

namespace HospitalApi.Services
{
    public interface IDoctorService
    {
        Task<PagedResult<DoctorDto>> GetAllAsync(DoctorQueryDto query);
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto> CreateAsync(DoctorCreateDto dto, int createdByUserId);
        Task<bool> UpdateAsync(int id, DoctorUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
