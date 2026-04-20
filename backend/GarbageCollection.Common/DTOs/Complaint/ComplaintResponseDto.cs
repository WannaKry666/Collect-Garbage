using GarbageCollection.Common.Models;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class ComplaintResponseDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = [];
        public string Status { get; set; } = string.Empty;
        public List<ComplaintMessage> Messages { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
