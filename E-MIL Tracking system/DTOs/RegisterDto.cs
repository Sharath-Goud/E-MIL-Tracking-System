using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace E_MIL_Tracking_system.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Designation { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }
    }
}