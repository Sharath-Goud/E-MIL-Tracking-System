using System.ComponentModel.DataAnnotations;

namespace E_MIL_Tracking_system.Models
{
    public class ChecklistRecord
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }

        [MaxLength(50)]
        public string? WeekCode { get; set; }

        [MaxLength(50)]
        public string? Month { get; set; }

        [MaxLength(100)]
        public string? Project { get; set; }

        [MaxLength(100)]
        public string? Line { get; set; }

        [MaxLength(100)]
        public string? Section { get; set; }

        [MaxLength(100)]
        public string? StationName { get; set; }

        [MaxLength(50)]
        public string? IssueType { get; set; }

        [MaxLength(2000)]
        public string? ProblemStatement { get; set; }

        [MaxLength(50)]
        public string? Frequency { get; set; }

        [MaxLength(50)]
        public string? IssueSeverity { get; set; }

        [MaxLength(200)]
        public string? Category { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(100)]
        public string? CmDri { get; set; }

        [MaxLength(100)]
        public string? RespectiveDepartment { get; set; }

        [MaxLength(100)]
        public string? AppleDri { get; set; }

        [MaxLength(50)]
        public string? TypeOfAudit { get; set; }

        [MaxLength(255)]
        public string? BeforeImagePath { get; set; }

        [MaxLength(2000)]
        public string? Rcca { get; set; }

        [MaxLength(500)]
        public string? AfterImagePath { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}