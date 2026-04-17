namespace GarbageCollection.Common.Models
{
    public class Citizen
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int TotalPoints { get; set; } = 0;

        // Navigation
        public ICollection<WasteReport> WasteReports { get; set; } = new List<WasteReport>();
    }
}
