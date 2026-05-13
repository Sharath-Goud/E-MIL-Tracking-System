namespace E_MIL_Tracking_system.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? EmpId { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Designation { get; set; }
        public string? Role { get; set; }
        public string? ProfileImagePath { get; set; }
        public bool IsAccessed { get; set; }
    }
}