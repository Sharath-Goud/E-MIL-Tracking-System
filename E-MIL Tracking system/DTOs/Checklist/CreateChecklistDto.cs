using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace E_MIL_Tracking_system.DTOs.Checklist
{
    public class CreateChecklistDto
    {
        [Required(ErrorMessage = "Date is required.")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Week code is required.")]
        public string? WeekCode { get; set; }

        [Required(ErrorMessage = "Month is required.")]
        public string? Month { get; set; }

        [Required(ErrorMessage = "Project is required.")]
        public string? Project { get; set; }

        [Required(ErrorMessage = "Line is required.")]
        public string? Line { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        public string? Section { get; set; }

        [Required(ErrorMessage = "Station Name is required.")]
        public string? StationName { get; set; }

        [Required(ErrorMessage = "Issue Type is required.")]
        public string? IssueType { get; set; }

        [Required(ErrorMessage = "Problem Statement is required.")]
        public string? ProblemStatement { get; set; }

        [Required(ErrorMessage = "Frequency is required.")]
        public string? Frequency { get; set; }

        [Required(ErrorMessage = "Issue Severity is required.")]
        public string? IssueSeverity { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "CM DRI is required.")]
        public string? CmDri { get; set; }

        [Required(ErrorMessage = "Respective Department is required.")]
        public string? RespectiveDepartment { get; set; }

        [Required(ErrorMessage = "Apple DRI is required.")]
        public string? AppleDri { get; set; }

        [Required(ErrorMessage = "Type of Audit is required.")]
        public string? TypeOfAudit { get; set; }
        [Required(ErrorMessage = "Auditor is required.")]
        public string? Auditor { get; set; }

        [Required(ErrorMessage = "Before Image is required.")]
        public List<IFormFile>? BeforeImages { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string? Status { get; set; }
    }
}