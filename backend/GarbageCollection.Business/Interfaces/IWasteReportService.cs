using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;

namespace GarbageCollection.Business.Interfaces
{
    public interface IWasteReportService
    {
        Task<WasteReportResponseDto> CreateReportAsync(int citizenId, CreateWasteReportDto dto);
        Task<WasteReportResponseDto?> GetReportByIdAsync(int id);
        Task<IEnumerable<WasteReportResponseDto>> GetReportsByCitizenAsync(int citizenId, ReportStatus? status = null);
        Task<CitizenReportsResult> GetCitizenReportsPagedAsync(int citizenId, int page, int limit);
        Task CancelReportAsync(int citizenId, int reportId);
        Task<WasteReportResponseDto> UpdateStatusAsync(int reportId, ReportStatus newStatus);
    }
}
