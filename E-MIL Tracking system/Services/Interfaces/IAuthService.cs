using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Models;

namespace E_MIL_Tracking_system.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> GetUserAsync(string empId, string password);
        Task<bool> UserExistsAsync(string empId);
        Task<bool> RegisterAsync(RegisterDto model, string webRootPath);
        Task<List<User>> GetOnlyUsersAsync();
        Task<bool> UpdateUserAccessAsync(int userId, bool isAccessed);
    }
}