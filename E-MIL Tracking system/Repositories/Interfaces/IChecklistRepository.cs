using E_MIL_Tracking_system.DTOs;
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
        Task<List<AuditHourDto>> GetAuditHoursAsync();
        Task DeleteApprovalsAsync(int checklistId);
        Task InsertApprovalAsync(int checklistId, string email);
        Task MarkAcceptedAsync(int checklistId, string email);
        Task MarkRejectedAsync(int checklistId, string email);
        Task<bool> AreAllAcceptedAsync(int checklistId);
        Task UpdateMainChecklistAsync(CreateChecklistDto dto, string beforeImagePath);
        Task<List<ChecklistApprovalReminderDto>> GetPendingReviewReminderAsync();
        Task MarkReviewReminderSentAsync(int checklistId, string email);
    }
}   