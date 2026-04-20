using GarbageCollection.Common.Enums;

namespace GarbageCollection.Common.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public int ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public List<ComplaintMessage> Messages { get; set; } = [];
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public WasteReport Report { get; set; } = null!;
        public Citizen Citizen { get; set; } = null!;
    }
}
