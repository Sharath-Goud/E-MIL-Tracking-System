using E_MIL_Tracking_system.DTOs.Checklist;

namespace E_MIL_Tracking_system.DTOs
{
    public class AuditHourDto
    {
        public string? AuditDate { get; set; }
        public DateTime? ParsedDate { get; set; }
        public decimal AuditHour { get; set; }
    }

    public class AuditHoursPageDto
    {
        public List<ChecklistResponseDto> ChecklistRecords { get; set; } = new();
        public List<AuditHourDto> AuditHours { get; set; } = new();
    }
}