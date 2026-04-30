using System.ComponentModel.DataAnnotations;

namespace E_MIL_Tracking_system.DTOs.Checklist
{
    public class UpdateChecklistRecordDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string? Rcca { get; set; }

    }
}
