namespace E_MIL_Tracking_system.DTOs
{
    public class SendSelectedChecklistMailDto
    {
        public List<int> Ids { get; set; } = new List<int>();
        public string Type { get; set; }
    }
}
