using HospitalApi.Dtos;
using HospitalApi.Helpers;

namespace HospitalApi.Services
{
    public interface IBillService
    {
        Task<PagedResult<BillDto>> GetAllAsync(BillQueryDto query);
        Task<BillDto?> GetByIdAsync(int id);
        Task<(BillDto? Result, string? Error)> CreateAsync(BillCreateDto dto, int createdByUserId);
        Task<(bool Success, string? Error)> UpdateAsync(int id, BillUpdateDto dto, int updatedByUserId);
        Task<(bool Success, string? Error)> PayAsync(int id, BillPayDto dto, int updatedByUserId);
        Task<bool> DeleteAsync(int id);
    }
}