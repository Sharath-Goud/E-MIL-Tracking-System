namespace E_MIL_Tracking_system.DTOs.Checklist
{
    public class ChecklistApprovalReminderDto
    {
        public int ChecklistId { get; set; }
        public string? Email { get; set; }
        public string? Section { get; set; }
        public string? StationName { get; set; }
        public string? IssueType { get; set; }
        public string? ProblemStatement { get; set; }
        public string? Frequency { get; set; }
        public string? IssueSeverity { get; set; }
        public string? Category { get; set; }
        public string? Rcca { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DueDate { get; set; }
        public string? CmDri { get; set; }
        public string? AppleDri { get; set; }
        public string? TypeOfAudit { get; set; }
        public string? BeforeImagePath { get; set; }
        public string? AfterImagePath { get; set; }
        public string? Status { get; set; }
    }
}
