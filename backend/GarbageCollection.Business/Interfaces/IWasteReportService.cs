using GarbageCollection.Common.DTOs.WasteReport;

namespace GarbageCollection.Business.Interfaces
{
    public interface IWasteReportService
    {
        Task<WasteReportResponseDto> CreateReportAsync(int citizenId, CreateWasteReportDto dto);
        Task<WasteReportResponseDto?> GetReportByIdAsync(int id);
        Task<IEnumerable<WasteReportResponseDto>> GetReportsByCitizenAsync(int citizenId);
    }
}
