namespace E_MIL_Tracking_system.DTOs.Checklist
{
    public class ChecklistResponseDto
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public string? WeekCode { get; set; }
        public string? Month { get; set; }
        public string? Project { get; set; }
        public string? Line { get; set; }
        public string? Section { get; set; }
        public string? StationName { get; set; }
        public string? IssueType { get; set; }
        public string? ProblemStatement { get; set; }
        public string? Frequency { get; set; }
        public string? IssueSeverity { get; set; }
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public string? CmDri { get; set; }
        public string? RespectiveDepartment { get; set; }
        public string? AppleDri { get; set; }
        public string? TypeOfAudit { get; set; }
        public string? BeforeImagePath { get; set; }
        public string? Rcca { get; set; }
        public string? AfterImagePath { get; set; }
        public string? Status { get; set; }
        public bool DueReminderSent { get; set; }
    }
}