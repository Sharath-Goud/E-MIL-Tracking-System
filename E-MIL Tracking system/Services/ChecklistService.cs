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
            var beforeImagePaths = new List<string>();

            if (dto.BeforeImages != null && dto.BeforeImages.Any())
            {
                dto.Status = "Ongoing";

                var folder = Path.Combine(webRootPath, "uploads", "checklist");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                foreach (var image in dto.BeforeImages)
                {
                    if (image == null || image.Length == 0)
                        continue;

                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var fullPath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    beforeImagePaths.Add("/uploads/checklist/" + fileName);
                }
            }

            string imagePath = string.Join(",", beforeImagePaths);
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

        public async Task MarkRejectedAsync(int checklistId, string email)
        {
            await _repo.MarkRejectedAsync(checklistId, email);
        }

        public async Task<bool> AreAllAcceptedAsync(int checklistId)
        {
            return await _repo.AreAllAcceptedAsync(checklistId);
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

        public async Task<List<ChecklistResponseDto>> GetPendingRccaReminderRecordsAsync()
        {
            var list = new List<ChecklistResponseDto>();

            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        SELECT *
        FROM ChecklistRecords
        WHERE CreatedAt IS NOT NULL
          AND DATEADD(HOUR, 6, CreatedAt) <= GETDATE()
          AND ISNULL(IsRccaMailSent, 0) = 0
          AND (Rcca IS NULL OR LTRIM(RTRIM(Rcca)) = '')
          AND ISNULL(Status, '') <> 'Closed'
    ";

            using SqlCommand cmd = new SqlCommand(query, con);
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ChecklistResponseDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Date = reader["Date"] == DBNull.Value ? null : Convert.ToDateTime(reader["Date"]),
                    WeekCode = reader["WeekCode"]?.ToString(),
                    Month = reader["Month"]?.ToString(),
                    Project = reader["Project"]?.ToString(),
                    Line = reader["Line"]?.ToString(),
                    Section = reader["Section"]?.ToString(),
                    StationName = reader["StationName"]?.ToString(),
                    IssueType = reader["IssueType"]?.ToString(),
                    ProblemStatement = reader["ProblemStatement"]?.ToString(),
                    Frequency = reader["Frequency"]?.ToString(),
                    IssueSeverity = reader["IssueSeverity"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    DueDate = reader["DueDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DueDate"]),
                    CmDri = reader["CmDri"]?.ToString(),
                    RespectiveDepartment = reader["RespectiveDepartment"]?.ToString(),
                    AppleDri = reader["AppleDri"]?.ToString(),
                    TypeOfAudit = reader["TypeOfAudit"]?.ToString(),
                    Auditor = reader["Auditor"]?.ToString(),
                    BeforeImagePath = reader["BeforeImagePath"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    CreatedAt = reader["CreatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    IsRccaMailSent = reader["IsRccaMailSent"] != DBNull.Value && Convert.ToBoolean(reader["IsRccaMailSent"])
                });
            }

            return list;
        }

        public async Task MarkRccaReminderMailSentAsync(int id)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            string query = @"
        UPDATE ChecklistRecords
        SET IsRccaMailSent = 1
        WHERE Id = @Id
    ";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ChecklistResponseDto>> GetPendingAfterImageReminderRecordsAsync()
        {
            var list = new List<ChecklistResponseDto>();
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = @"
                SELECT *
                FROM ChecklistRecords
                WHERE RccaUpdatedAt IS NOT NULL
                  AND DATEADD(HOUR, 4, RccaUpdatedAt) <= GETDATE()
                  AND ISNULL(IsAfterImageReminderSent, 0) = 0
                  AND (AfterImagePath IS NULL OR LTRIM(RTRIM(AfterImagePath)) = '')
                  AND ISNULL(Status, '') <> 'Closed'
            ";
            using SqlCommand cmd = new SqlCommand(query, con);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ChecklistResponseDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Date = reader["Date"] == DBNull.Value ? null : Convert.ToDateTime(reader["Date"]),
                    Section = reader["Section"]?.ToString(),
                    StationName = reader["StationName"]?.ToString(),
                    IssueType = reader["IssueType"]?.ToString(),
                    ProblemStatement = reader["ProblemStatement"]?.ToString(),
                    Frequency = reader["Frequency"]?.ToString(),
                    IssueSeverity = reader["IssueSeverity"]?.ToString(),
                    Category = reader["Category"]?.ToString(),
                    DueDate = reader["DueDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DueDate"]),
                    CmDri = reader["CmDri"]?.ToString(),
                    AppleDri = reader["AppleDri"]?.ToString(),
                    TypeOfAudit = reader["TypeOfAudit"]?.ToString(),
                    Auditor = reader["Auditor"]?.ToString(),
                    Rcca = reader["Rcca"]?.ToString(),
                    Status = reader["Status"]?.ToString()
                });
            }
            return list;
        }

        public async Task MarkAfterImageReminderSentAsync(int id)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = @"
                UPDATE ChecklistRecords
                SET IsAfterImageReminderSent = 1
                WHERE Id = @Id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}