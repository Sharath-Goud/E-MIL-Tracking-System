using E_MIL_Tracking_system.Data;
using E_MIL_Tracking_system.DTOs;
using E_MIL_Tracking_system.Models;
using E_MIL_Tracking_system.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace E_MIL_Tracking_system.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DbContext _db;

        public AuthRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetUserAsync(string empId, string password)
        {
            using var con = _db.CreateConnection();

            string query = @"
                SELECT Id, EmpId, FullName, Password, Designation, Role, ProfileImagePath, IsAccessed
                FROM Users
                WHERE EmpId = @EmpId AND Password = @Password";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@EmpId", empId);
            cmd.Parameters.AddWithValue("@Password", password);

            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    EmpId = reader["EmpId"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Password = reader["Password"].ToString(),
                    Designation = reader["Designation"].ToString(),
                    Role = reader["Role"].ToString(),
                    ProfileImagePath = reader["ProfileImagePath"]?.ToString(),
                    IsAccessed = reader["IsAccessed"] != DBNull.Value && Convert.ToBoolean(reader["IsAccessed"])
                };
            }

            return null;
        }

        public async Task<bool> UserExistsAsync(string empId)
        {
            using var con = _db.CreateConnection();

            string query = "SELECT COUNT(1) FROM Users WHERE EmpId = @EmpId";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@EmpId", empId);

            await con.OpenAsync();

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<bool> RegisterAsync(RegisterDto model, string? imagePath)
        {
            using var con = _db.CreateConnection();

            string query = @"
                INSERT INTO Users
                (
                    EmpId,
                    FullName,
                    Password,
                    Designation,
                    Role,
                    ProfileImagePath,
                    CreatedAt
                )
                VALUES
                (
                    @EmpId,
                    @FullName,
                    @Password,
                    @Designation,
                    @Role,
                    @ProfileImagePath,
                    GETDATE()
                )";

            using var cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@EmpId", model.UserId);
            cmd.Parameters.AddWithValue("@FullName", model.FullName);
            cmd.Parameters.AddWithValue("@Password", model.Password);
            cmd.Parameters.AddWithValue("@Designation", model.Designation);
            cmd.Parameters.AddWithValue("@Role", model.Role);
            cmd.Parameters.AddWithValue("@ProfileImagePath", string.IsNullOrEmpty(imagePath) ? DBNull.Value : imagePath);

            await con.OpenAsync();

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<List<User>> GetOnlyUsersAsync()
        {
            var users = new List<User>();

            using var con = _db.CreateConnection();

            string query = @"
        SELECT Id, EmpId, FullName, Role, IsAccessed
        FROM Users
        WHERE Role = 'User'
        ORDER BY FullName";

            using var cmd = new SqlCommand(query, con);

            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    EmpId = reader["EmpId"]?.ToString(),
                    FullName = reader["FullName"]?.ToString(),
                    Role = reader["Role"]?.ToString(),
                    IsAccessed = reader["IsAccessed"] != DBNull.Value && Convert.ToBoolean(reader["IsAccessed"])
                });
            }

            return users;
        }

        public async Task<bool> UpdateUserAccessAsync(int userId, bool isAccessed)
        {
            using var con = _db.CreateConnection();

            string query = @"
                UPDATE Users
                SET IsAccessed = @IsAccessed
                WHERE Id = @Id";

            using var cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Id", userId);
            cmd.Parameters.AddWithValue("@IsAccessed", isAccessed);

            await con.OpenAsync();

            int rows = await cmd.ExecuteNonQueryAsync();

            return rows > 0;
        }
    }
}