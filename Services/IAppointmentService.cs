using HospitalApi.Dtos;
using HospitalApi.Helpers;

namespace HospitalApi.Services
{
    public interface IAppointmentService
    {
        Task<PagedResult<AppointmentDto>> GetAllAsync(AppointmentQueryDto query);
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<(AppointmentDto? Result, string? Error)> CreateAsync(AppointmentCreateDto dto, int createdByUserId);
        Task<(bool Success, string? Error)> UpdateAsync(int id, AppointmentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
