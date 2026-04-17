using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.WasteReport;
using GarbageCollection.Common.Enums;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WasteReportController : ControllerBase
    {
        private readonly IWasteReportService _wasteReportService;

        public WasteReportController(IWasteReportService wasteReportService)
        {
            _wasteReportService = wasteReportService;
        }

        /// <summary>
        /// Citizen gửi báo cáo rác mới (multipart/form-data).
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WasteReportResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReport([FromForm] CreateWasteReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));

            // TODO: Lấy citizenId từ JWT claims thay vì hardcode
            var citizenId = GetCurrentCitizenId();

            var result = await _wasteReportService.CreateReportAsync(citizenId, dto);
            return CreatedAtAction(nameof(GetReportById), new { id = result.Id },
                ApiResponse<WasteReportResponseDto>.Ok(result, "Tạo báo cáo thành công."));
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
                return NotFound(ApiResponse<object>.Fail($"Không tìm thấy báo cáo với ID {id}."));

            return Ok(ApiResponse<WasteReportResponseDto>.Ok(result));
        }

        /// <summary>
        /// Lấy danh sách báo cáo của Citizen đang đăng nhập.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái: Pending, Accepted, Assigned, Collected (bỏ trống = lấy tất cả)</param>
        [HttpGet("my-reports")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WasteReportResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReports([FromQuery] ReportStatus? status = null)
        {
            var citizenId = GetCurrentCitizenId();
            var results = await _wasteReportService.GetReportsByCitizenAsync(citizenId, status);
            return Ok(ApiResponse<IEnumerable<WasteReportResponseDto>>.Ok(results));
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
