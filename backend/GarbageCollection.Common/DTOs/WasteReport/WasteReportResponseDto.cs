namespace GarbageCollection.Common.DTOs.WasteReport
{
    public class WasteReportResponseDto
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public string CitizenName { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public string Description { get; set; } = string.Empty;
        public List<string> WasteTypes { get; set; } = [];
        public string Size { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
