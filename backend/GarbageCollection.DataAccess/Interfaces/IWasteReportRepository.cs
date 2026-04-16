using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface IWasteReportRepository
    {
        Task<WasteReport> CreateAsync(WasteReport report);
        Task<WasteReport?> GetByIdAsync(int id);
        Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId, ReportStatus? status = null);
        Task<WasteReport> UpdateAsync(WasteReport report);
    }
}
