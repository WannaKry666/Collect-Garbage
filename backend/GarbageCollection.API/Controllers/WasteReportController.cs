using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WasteReportController : ControllerBase
    {
        private readonly IWasteReportService _wasteReportService;

        public WasteReportController(IWasteReportService wasteReportService)
        {
            _wasteReportService = wasteReportService;
        }

        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Citizen gửi báo cáo rác mới (multipart/form-data).
        /// </summary>
        [HttpPost("/api/v1/users/citizen-reports")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WasteReportResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status413RequestEntityTooLarge)]
        public async Task<IActionResult> CreateReport([FromForm] CreateWasteReportDto dto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT"));

            if (dto.Images.Count < 1)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Vui lòng gửi ít nhất 1 ảnh."));

            if (dto.Images.Count > 5)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Tối đa 5 ảnh mỗi lần gửi."));

            if (dto.Type.Count < 1)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_INPUT", "Vui lòng chọn ít nhất 1 loại rác."));

            var invalidFormat = dto.Images.FirstOrDefault(f =>
                !AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()));
            if (invalidFormat != null)
                return UnprocessableEntity(ApiResponse<object>.Fail("invalid input data", "INVALID_FILE_FORMAT",
                    $"Định dạng không hợp lệ: {Path.GetExtension(invalidFormat.FileName)}. Chỉ chấp nhận jpg, jpeg, png."));

            var oversized = dto.Images.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
            if (oversized != null)
                return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                    ApiResponse<object>.Fail("file too large", "FILE_TOO_LARGE", "Mỗi ảnh tối đa 5MB."));

            // TODO: Lấy citizenId từ JWT claims thay vì hardcode
            var citizenId = GetCurrentCitizenId();

            var result = await _wasteReportService.CreateReportAsync(citizenId, dto);
            return CreatedAtAction(nameof(GetReportById), new { id = result.ReportId },
                ApiResponse<WasteReportResponseDto>.Ok(result, "report created successfully"));
        }

        /// <summary>
        /// Lấy chi tiết một báo cáo theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<WasteReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportById(int id)
        {
            var result = await _wasteReportService.GetReportByIdAsync(id);
            if (result is null)
                return NotFound(ApiResponse<object>.Fail($"Không tìm thấy báo cáo với ID {id}.", "NOT_FOUND"));

            return Ok(ApiResponse<WasteReportResponseDto>.Ok(result));
        }

        /// <summary>
        /// Lấy danh sách báo cáo của Citizen đang đăng nhập, hỗ trợ phân trang.
        /// </summary>
        /// <param name="page">Trang hiện tại, bắt đầu từ 1 (mặc định: 1)</param>
        /// <param name="limit">Số bản ghi mỗi trang, tối đa 50 (mặc định: 10)</param>
        [HttpGet("/api/v1/users/citizen-reports")]
        [ProducesResponseType(typeof(ApiResponse<CitizenReportsResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCitizenReports([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (page < 1 || limit < 1 || limit > 50)
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    "invalid query params",
                    "INVALID_QUERY_PARAMS",
                    "page >= 1, limit must be between 1 and 50"));

            var citizenId = GetCurrentCitizenId();
            var result = await _wasteReportService.GetCitizenReportsPagedAsync(citizenId, page, limit);
            return Ok(ApiResponse<CitizenReportsResult>.Ok(result, "get citizen reports successfully"));
        }

        /// <summary>
        /// Citizen hủy báo cáo — chỉ được khi status là Pending.
        /// </summary>
        /// <param name="id">ID của báo cáo</param>
        [HttpDelete("/api/v1/users/citizen-reports/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelReport(int id)
        {
            var citizenId = GetCurrentCitizenId();
            await _wasteReportService.CancelReportAsync(citizenId, id);
            return Ok(ApiResponse<object>.Ok(null!, "report cancelled successfully"));
        }

        /// <summary>
        /// Cập nhật trạng thái báo cáo theo luồng: Pending → Accepted → Assigned → Collected.
        /// </summary>
        /// <param name="id">ID của báo cáo</param>
        /// <param name="newStatus">Trạng thái mới</param>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ApiResponse<WasteReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] ReportStatus newStatus)
        {
            // TODO: Kiểm tra role — chỉ Enterprise/Collector mới được đổi status
            var result = await _wasteReportService.UpdateStatusAsync(id, newStatus);
            return Ok(ApiResponse<WasteReportResponseDto>.Ok(result, "Cập nhật trạng thái thành công."));
        }

        // Tạm thời hardcode, sẽ thay bằng JWT claim sau
        private static int GetCurrentCitizenId() => 1;
    }
}
