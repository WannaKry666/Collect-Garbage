namespace GarbageCollection.Common.DTOs.WasteReport
{
    public class CitizenReportsResult
    {
        public List<WasteReportResponseDto> Reports { get; set; } = [];
        public PaginationMeta Pagination { get; set; } = new();
    }
}
