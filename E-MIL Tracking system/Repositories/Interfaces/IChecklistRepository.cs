using E_MIL_Tracking_system.DTOs.Checklist;

namespace E_MIL_Tracking_system.Repositories.Interfaces
{
    public interface IChecklistRepository
    {
        Task<int> InsertAsync(CreateChecklistDto dto, string imagePath);
        Task<List<ChecklistResponseDto>> GetAllAsync();
        Task<ChecklistResponseDto?> GetByIdAsync(int id);
        Task UpdateChecklistRecordAsync(int id, string? rcca);
        Task UpdateAfterImageAsync(int id, string afterImagePath);
        Task UpdateStatusAsync(int id, string status);
    }
}