using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Models;

namespace E_MIL_Tracking_system.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserAsync(string empId, string password);
        Task<bool> UserExistsAsync(string empId);
        Task<bool> RegisterAsync(RegisterDto model, string? imagePath);
    }
}