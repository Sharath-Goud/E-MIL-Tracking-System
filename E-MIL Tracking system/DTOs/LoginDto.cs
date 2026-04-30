using System.ComponentModel.DataAnnotations;

namespace E_MIL_Tracking_system.DTOs
{
    public class LoginDto
    {
        [Required]
        public required string EmpId { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}