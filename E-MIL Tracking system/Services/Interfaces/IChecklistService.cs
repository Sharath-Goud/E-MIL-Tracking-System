using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.DTOs.Checklist;

namespace E_MIL_Tracking_system.Services.Interfaces
{
    public interface IChecklistService
    {
        Task<int> SaveAsync(CreateChecklistDto dto, string webRootPath);
        Task<List<ChecklistResponseDto>> GetAllAsync();
        Task<ChecklistResponseDto?> GetByIdAsync(int id);
        Task<List<AuditHourDto>> GetAuditHoursAsync();
    }
}