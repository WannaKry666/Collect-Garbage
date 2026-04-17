using Microsoft.EntityFrameworkCore;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.DataAccess.Repositories
{
    public class WasteReportRepository : IWasteReportRepository
    {
        private readonly AppDbContext _context;

        public WasteReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WasteReport> CreateAsync(WasteReport report)
        {
            _context.WasteReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<WasteReport?> GetByIdAsync(int id)
        {
            return await _context.WasteReports
                .Include(r => r.Citizen)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId, ReportStatus? status = null)
        {
            var query = _context.WasteReports
                .Include(r => r.Citizen)
                .Where(r => r.CitizenId == citizenId);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<WasteReport> Items, int Total)> GetByCitizenIdPagedAsync(int citizenId, int page, int limit)
        {
            var query = _context.WasteReports
                .Include(r => r.Citizen)
                .Where(r => r.CitizenId == citizenId)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (items, total);
        }

        public async Task<WasteReport> UpdateAsync(WasteReport report)
        {
            report.UpdatedAt = DateTime.UtcNow;
            _context.WasteReports.Update(report);
            await _context.SaveChangesAsync();
            return report;
        }
    }
}
