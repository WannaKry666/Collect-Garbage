using GarbageCollection.Common.Enums;

namespace GarbageCollection.Common.Models
{
    public class WasteReport
    {
        public int ReportId { get; set; }

        public int CitizenId { get; set; }

        public List<string> ImageUrls { get; set; } = [];

        public string? Description { get; set; }

        public List<WasteType> WasteTypes { get; set; } = [];

        public WasteSize? Size { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Citizen Citizen { get; set; } = null!;
    }
}
