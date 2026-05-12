namespace E_MIL_Tracking_system.DTOs
{
    public class AuditTableRowDto
    {
        public int WeekNumber { get; set; }
        public List<string> Dates { get; set; }
        public List<string> TotalFindings { get; set; }
        public List<string> AuditHours { get; set; }
        public List<string> FindingHr { get; set; }
        public List<string> RValues { get; set; }
    }
}
