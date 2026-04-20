using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;
using GarbageCollection.Common.Exceptions;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;

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
            if (dto.Type.Count > 4)
                throw new ArgumentException("Tối đa 4 loại rác mỗi báo cáo.");

            var imageUrls = await _cloudinaryService.UploadImagesAsync(dto.Images, "waste-reports");

            var report = new WasteReport
            {
                CitizenId = citizenId,
                ImageUrls = imageUrls,
                Description = dto.Description,
                WasteTypes = dto.Type.ToList(),
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

        public async Task<CitizenReportsResult> GetCitizenReportsPagedAsync(int citizenId, int page, int limit)
        {
            var (items, total) = await _reportRepository.GetByCitizenIdPagedAsync(citizenId, page, limit);
            return new CitizenReportsResult
            {
                Reports = items.Select(MapToResponse).ToList(),
                Pagination = new PaginationMeta
                {
                    Page = page,
                    Limit = limit,
                    Total = total,
                    TotalPages = (int)Math.Ceiling((double)total / limit)
                }
            };
        }

        public async Task CancelReportAsync(int citizenId, int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            if (report.CitizenId != citizenId)
                throw new UnauthorizedAccessException("you are not allowed to cancel this report");

            if (report.Status != ReportStatus.Pending)
                throw new InvalidOperationException("cannot cancel report that is not pending");

            await _reportRepository.DeleteAsync(report);
        }

        public async Task<WasteReportResponseDto> UpdateReportAsync(int citizenId, int reportId, UpdateWasteReportDto dto)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("report not found");

            if (report.CitizenId != citizenId)
                throw new UnauthorizedAccessException("you are not allowed to update this report");

            if (report.Status != ReportStatus.Pending)
                throw new InvalidOperationException("cannot update report that is not pending");

            // Chỉ cho phép update 1 lần (updated_at phải còn null)
            if (report.UpdatedAt.HasValue)
                throw new TooManyRequestsException("too many request");

            // Upload ảnh mới và xóa ảnh cũ nếu có gửi ảnh
            if (dto.Images != null && dto.Images.Count > 0)
            {
                await _cloudinaryService.DeleteImagesAsync(report.ImageUrls);
                report.ImageUrls = await _cloudinaryService.UploadImagesAsync(dto.Images, "waste-reports");
            }

            if (dto.Type != null && dto.Type.Count > 0)
                report.WasteTypes = dto.Type.ToList();

            if (dto.Size.HasValue)
                report.Size = dto.Size;

            if (dto.Description != null)
                report.Description = dto.Description;

            var updated = await _reportRepository.UpdateAsync(report);
            return MapToResponse(updated);
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
            ReportId    = report.ReportId,
            ImageUrls   = report.ImageUrls,
            Type        = report.WasteTypes.Select(w => w.ToString()).ToList(),
            Size        = report.Size?.ToString(),
            Description = report.Description,
            Status      = report.Status.ToString(),
            CreatedAt   = report.CreatedAt,
            UpdatedAt   = report.UpdatedAt
        };
    }
}
