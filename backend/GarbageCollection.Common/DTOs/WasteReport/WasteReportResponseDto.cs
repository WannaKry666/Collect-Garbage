namespace GarbageCollection.Common.DTOs.WasteReport
{
    public class WasteReportResponseDto
    {
        public int ReportId { get; set; }
        public List<string> ImageUrls { get; set; } = [];
        public List<string> Type { get; set; } = [];
        public string? Size { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
