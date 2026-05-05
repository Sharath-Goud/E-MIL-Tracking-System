using E_MIL_Tracking_system.Data;
using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.DTOs.Checklist;
using E_MIL_Tracking_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace E_MIL_Tracking_system.Repositories
{
    public class ChecklistRepository : IChecklistRepository
    {
        private readonly DbContext _db;

        public ChecklistRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> InsertAsync(CreateChecklistDto dto, string imagePath)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
        INSERT INTO ChecklistRecords
        ([Date], WeekCode, [Month], Project, [Line], Section, StationName, IssueType,
         ProblemStatement, Frequency, IssueSeverity, Category, DueDate,
         CmDri, RespectiveDepartment, AppleDri, TypeOfAudit, BeforeImagePath, Status)
        OUTPUT INSERTED.Id
        VALUES
        (@Date, @WeekCode, @Month, @Project, @Line, @Section, @StationName, @IssueType,
         @ProblemStatement, @Frequency, @IssueSeverity, @Category, @DueDate,
         @CmDri, @RespectiveDepartment, @AppleDri, @TypeOfAudit, @BeforeImagePath, @Status)";

            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = (object?)dto.Date ?? DBNull.Value;
            cmd.Parameters.Add("@WeekCode", SqlDbType.NVarChar).Value = (object?)dto.WeekCode ?? DBNull.Value;
            cmd.Parameters.Add("@Month", SqlDbType.NVarChar).Value = (object?)dto.Month ?? DBNull.Value;
            cmd.Parameters.Add("@Project", SqlDbType.NVarChar).Value = (object?)dto.Project ?? DBNull.Value;
            cmd.Parameters.Add("@Line", SqlDbType.NVarChar).Value = (object?)dto.Line ?? DBNull.Value;
            cmd.Parameters.Add("@Section", SqlDbType.NVarChar).Value = (object?)dto.Section ?? DBNull.Value;
            cmd.Parameters.Add("@StationName", SqlDbType.NVarChar).Value = (object?)dto.StationName ?? DBNull.Value;
            cmd.Parameters.Add("@IssueType", SqlDbType.NVarChar).Value = (object?)dto.IssueType ?? DBNull.Value;
            cmd.Parameters.Add("@ProblemStatement", SqlDbType.NVarChar).Value = (object?)dto.ProblemStatement ?? DBNull.Value;
            cmd.Parameters.Add("@Frequency", SqlDbType.NVarChar).Value = (object?)dto.Frequency ?? DBNull.Value;
            cmd.Parameters.Add("@IssueSeverity", SqlDbType.NVarChar).Value = (object?)dto.IssueSeverity ?? DBNull.Value;
            cmd.Parameters.Add("@Category", SqlDbType.NVarChar).Value = (object?)dto.Category ?? DBNull.Value;
            cmd.Parameters.Add("@DueDate", SqlDbType.DateTime).Value = (object?)dto.DueDate ?? DBNull.Value;
            cmd.Parameters.Add("@CmDri", SqlDbType.NVarChar).Value = (object?)dto.CmDri ?? DBNull.Value;
            cmd.Parameters.Add("@RespectiveDepartment", SqlDbType.NVarChar).Value = (object?)dto.RespectiveDepartment ?? DBNull.Value;
            cmd.Parameters.Add("@AppleDri", SqlDbType.NVarChar).Value = (object?)dto.AppleDri ?? DBNull.Value;
            cmd.Parameters.Add("@TypeOfAudit", SqlDbType.NVarChar).Value = (object?)dto.TypeOfAudit ?? DBNull.Value;
            cmd.Parameters.Add("@BeforeImagePath", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(imagePath) ? DBNull.Value : imagePath;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(dto.Status) ? DBNull.Value : dto.Status;

            var insertedId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(insertedId);
        }

        public async Task<List<ChecklistResponseDto>> GetAllAsync()
        {
            var result = new List<ChecklistResponseDto>();

            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, [Date], WeekCode, [Month], Project, [Line], Section, StationName,
                       IssueType, ProblemStatement, Frequency, IssueSeverity, Category, DueDate,
                       CmDri, RespectiveDepartment, AppleDri, TypeOfAudit,
                       BeforeImagePath, Rcca, AfterImagePath, Status
                FROM ChecklistRecords
                ORDER BY Id DESC";

            using var cmd = new SqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ChecklistResponseDto
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
                    BeforeImagePath = reader["BeforeImagePath"]?.ToString(),
                    Rcca = reader["Rcca"]?.ToString(),
                    AfterImagePath = reader["AfterImagePath"]?.ToString(),
                    Status = reader["Status"]?.ToString()
                });
            }

            return result;
        }

        public async Task<ChecklistResponseDto?> GetByIdAsync(int id)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
        SELECT Id, [Date], WeekCode, [Month], Project, [Line], Section, StationName,
               IssueType, ProblemStatement, Frequency, IssueSeverity, Category, DueDate,
               CmDri, RespectiveDepartment, AppleDri, TypeOfAudit,
               BeforeImagePath, Rcca, AfterImagePath, Status
        FROM ChecklistRecords
        WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ChecklistResponseDto
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
                    BeforeImagePath = reader["BeforeImagePath"]?.ToString(),
                    Rcca = reader["Rcca"]?.ToString(),
                    AfterImagePath = reader["AfterImagePath"]?.ToString(),
                    Status = reader["Status"]?.ToString()
                };
            }

            return null;
        }

        public async Task UpdateChecklistRecordAsync(int id, string? rcca)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
        UPDATE ChecklistRecords
        SET Rcca = @Rcca
        WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Rcca", string.IsNullOrWhiteSpace(rcca) ? DBNull.Value : rcca);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAfterImageAsync(int id, string afterImagePath)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
            UPDATE ChecklistRecords
            SET AfterImagePath = @AfterImagePath,
                Status = 'Closed'
            WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@AfterImagePath", afterImagePath);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            var query = @"
            UPDATE ChecklistRecords
            SET Status = @Status
            WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Status", status);

            await cmd.ExecuteNonQueryAsync();
        }

        private DateTime? ParseAuditDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = text.Trim();

            if (text.StartsWith("WK"))
            {
                var parts = text.Split('/');

                if (parts.Length == 2)
                    text = parts[1];
            }

            if (DateTime.TryParseExact(
                text + "-" + DateTime.Now.Year,
                "dd-MMM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var date))
            {
                return date.Date;
            }

            return null;
        }

        public async Task<List<AuditHourDto>> GetAuditHoursAsync()
        {
            var list = new List<AuditHourDto>();

            using var con = _db.CreateConnection();
            await con.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT AuditDate, AuditHour
                FROM AuditHours
            ", con);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new AuditHourDto
                {
                    AuditDate = reader["AuditDate"] == DBNull.Value
                    ? ""
                    : reader["AuditDate"].ToString(),

                                ParsedDate = ParseAuditDate(reader["AuditDate"]?.ToString()),

                                AuditHour = reader["AuditHour"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(reader["AuditHour"])
                });
            }

            return list;
        }

        public async Task DeleteApprovalsAsync(int checklistId)
        {
            using var con = _db.CreateConnection();
            string query = "DELETE FROM ChecklistApprovalStatus WHERE ChecklistId = @ChecklistId";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChecklistId", checklistId);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertApprovalAsync(int checklistId, string email)
        {
            using var con = _db.CreateConnection();
            string query = @"
        INSERT INTO ChecklistApprovalStatus (ChecklistId, Email, IsAccepted, IsRejected)
        VALUES (@ChecklistId, @Email, 0, 0)";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChecklistId", checklistId);
            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkAcceptedAsync(int checklistId, string email)
        {
            using var con = _db.CreateConnection();
            string query = @"
        UPDATE ChecklistApprovalStatus
        SET IsAccepted = 1,
            IsRejected = 0,
            ActionDate = GETDATE()
        WHERE ChecklistId = @ChecklistId AND Email = @Email";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChecklistId", checklistId);
            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkRejectedAsync(int checklistId, string email)
        {
            using var con = _db.CreateConnection();
            string query = @"
        UPDATE ChecklistApprovalStatus
        SET IsAccepted = 0,
            IsRejected = 1,
            ActionDate = GETDATE()
        WHERE ChecklistId = @ChecklistId AND Email = @Email";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChecklistId", checklistId);
            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> AreAllAcceptedAsync(int checklistId)
        {
            using var con = _db.CreateConnection();
            string query = @"
        SELECT 
            CASE 
                WHEN COUNT(*) > 0 
                     AND SUM(CASE WHEN IsAccepted = 1 THEN 1 ELSE 0 END) = COUNT(*)
                THEN 1 
                ELSE 0 
            END
        FROM ChecklistApprovalStatus
        WHERE ChecklistId = @ChecklistId";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ChecklistId", checklistId);

            await con.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) == 1;
        }
    }
}