using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class WasteReportService : IWasteReportService
    {
        private readonly IWasteReportRepository _reportRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public WasteReportService(
            IWasteReportRepository reportRepository,
            ICloudinaryService cloudinaryService)
        {
            _reportRepository = reportRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<WasteReportResponseDto> CreateReportAsync(int citizenId, CreateWasteReportDto dto)
        {
            if (dto.WasteTypes.Count > 4)
                throw new ArgumentException("Tối đa 4 loại rác mỗi báo cáo.");

            var imageUrls = await _cloudinaryService.UploadImagesAsync(dto.Images, "waste-reports");

            var report = new WasteReport
            {
                CitizenId = citizenId,
                ImageUrls = imageUrls,
                Description = dto.Description,
                WasteTypes = dto.WasteTypes.ToList(),
                Size = dto.Size,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _reportRepository.CreateAsync(report);
            return MapToResponse(created);
        }

        public async Task<WasteReportResponseDto?> GetReportByIdAsync(int id)
        {
            var report = await _reportRepository.GetByIdAsync(id);
            return report is null ? null : MapToResponse(report);
        }

        public async Task<IEnumerable<WasteReportResponseDto>> GetReportsByCitizenAsync(int citizenId, ReportStatus? status = null)
        {
            var reports = await _reportRepository.GetByCitizenIdAsync(citizenId, status);
            return reports.Select(MapToResponse);
        }

        public async Task<WasteReportResponseDto> UpdateStatusAsync(int reportId, ReportStatus newStatus)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException($"Không tìm thấy báo cáo với ID {reportId}.");

            ValidateStatusTransition(report.Status, newStatus);

            report.Status = newStatus;
            var updated = await _reportRepository.UpdateAsync(report);
            return MapToResponse(updated);
        }

        private static void ValidateStatusTransition(ReportStatus current, ReportStatus next)
        {
            var allowed = new Dictionary<ReportStatus, ReportStatus>
            {
                { ReportStatus.Pending,  ReportStatus.Accepted  },  // Enterprise accepts
                { ReportStatus.Accepted, ReportStatus.Assigned  },  // Enterprise assigns Collector
                { ReportStatus.Assigned, ReportStatus.Collected },  // Collector confirms
            };

            if (!allowed.TryGetValue(current, out var expected) || expected != next)
                throw new InvalidOperationException(
                    $"Không thể chuyển trạng thái từ '{current}' sang '{next}'. " +
                    $"Trạng thái tiếp theo hợp lệ: '{(allowed.ContainsKey(current) ? allowed[current] : "không có")}'.");
        }

        private static WasteReportResponseDto MapToResponse(WasteReport report) => new()
        {
            Id = report.Id,
            CitizenId = report.CitizenId,
            CitizenName = report.Citizen?.FullName ?? string.Empty,
            ImageUrls = report.ImageUrls,
            Description = report.Description,
            WasteTypes = report.WasteTypes.Select(w => w.ToString()).ToList(),
            Size = report.Size.ToString(),
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
    }
}
