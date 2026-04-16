using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface IWasteReportService
    {
        Task<WasteReportResponseDto> CreateReportAsync(int citizenId, CreateWasteReportDto dto);
        Task<WasteReportResponseDto?> GetReportByIdAsync(int id);
        Task<IEnumerable<WasteReportResponseDto>> GetReportsByCitizenAsync(int citizenId, ReportStatus? status = null);
        Task<WasteReportResponseDto> UpdateStatusAsync(int reportId, UpdateReportStatusDto dto);
    }
}
