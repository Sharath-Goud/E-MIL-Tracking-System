using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.DTOs.Checklist;
using E_MIL_Tracking_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace E_MIL_Tracking_system.Services
{
    public class ChecklistService
    {
        private readonly IChecklistRepository _repo;
        private readonly IConfiguration _configuration;

        public ChecklistService(IChecklistRepository repo, IConfiguration configuration)
        {
            _repo = repo;
            _configuration = configuration;
        }

        public async Task<int> SaveAsync(CreateChecklistDto dto, string webRootPath)
        {
            string imagePath = "";

            if (dto.BeforeImage != null && dto.BeforeImage.Length > 0)
            {
                var folder = Path.Combine(webRootPath, "uploads");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.BeforeImage.FileName);
                var fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await dto.BeforeImage.CopyToAsync(stream);

                imagePath = "/uploads/" + fileName;
            }

            return await _repo.InsertAsync(dto, imagePath);
        }

        public async Task<List<ChecklistResponseDto>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<List<AuditHourDto>> GetAuditHoursAsync()
        {
            return await _repo.GetAuditHoursAsync();
        }

        public async Task<ChecklistResponseDto?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task UpdateChecklistRecordAsync(UpdateChecklistRecordDto dto)
        {
            var record = await _repo.GetByIdAsync(dto.Id);

            if (record == null)
            {
                throw new Exception("Checklist record not found");
            }

            await _repo.UpdateChecklistRecordAsync(dto.Id, dto.Rcca);
        }

        public async Task UploadAfterImageAsync(int id, IFormFile afterImage, string webRootPath)
        {
            var record = await _repo.GetByIdAsync(id);

            if (record == null)
            {
                throw new Exception("Checklist record not found");
            }

            string uploadsFolder = Path.Combine(webRootPath, "uploads", "checklist");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid() + Path.GetExtension(afterImage.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await afterImage.CopyToAsync(stream);

            string afterImagePath = "/uploads/checklist/" + fileName;

            await _repo.UpdateAfterImageAsync(id, afterImagePath);
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            var record = await _repo.GetByIdAsync(id);

            if (record == null)
            {
                throw new Exception("Checklist record not found");
            }

            await _repo.UpdateStatusAsync(id, status);
        }

        public async Task ResetApprovalsAsync(int checklistId, List<string> emails)
        {
            await _repo.DeleteApprovalsAsync(checklistId);

            foreach (var email in emails)
            {
                await _repo.InsertApprovalAsync(checklistId, email);
            }
        }

        public async Task MarkAcceptedAsync(int checklistId, string email)
        {
            await _repo.MarkAcceptedAsync(checklistId, email);
        }

        public async Task<bool> AreAllAcceptedAsync(int checklistId)
        {
            return await _repo.AreAllAcceptedAsync(checklistId);
        }

        public async Task MarkRejectedAsync(int checklistId, string email)
        {
            await _repo.MarkRejectedAsync(checklistId, email);
        }

        public async Task MarkDueReminderSentAsync(int id)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        UPDATE ChecklistRecords
        SET DueReminderSent = 1
        WHERE Id = @Id";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}