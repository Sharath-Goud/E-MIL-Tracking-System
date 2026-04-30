using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Models;
using E_MIL_Tracking_system.Repositories.Interfaces;
using E_MIL_Tracking_system.Services.Interfaces;

namespace E_MIL_Tracking_system.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;

        public AuthService(IAuthRepository repository)
        {
            _repository = repository;
        }

        public async Task<User?> GetUserAsync(string empId, string password)
        {
            return await _repository.GetUserAsync(empId, password);
        }

        public async Task<bool> UserExistsAsync(string empId)
        {
            return await _repository.UserExistsAsync(empId);
        }

        public async Task<bool> RegisterAsync(RegisterDto model, string webRootPath)
        {
            string? imagePath = null;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "users");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await model.ProfileImage.CopyToAsync(stream);

                imagePath = "/uploads/users/" + fileName;
            }

            return await _repository.RegisterAsync(model, imagePath);
        }
    }
}