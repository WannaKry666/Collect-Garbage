using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(typeof(WasteReportResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReport([FromForm] CreateWasteReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // TODO: Lấy citizenId từ JWT claims thay vì hardcode
            var citizenId = GetCurrentCitizenId();

            var result = await _wasteReportService.CreateReportAsync(citizenId, dto);
            return CreatedAtAction(nameof(GetReportById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Lấy chi tiết một báo cáo theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(WasteReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportById(int id)
        {
            var result = await _wasteReportService.GetReportByIdAsync(id);
            if (result is null)
                return NotFound(new { message = $"Không tìm thấy báo cáo với ID {id}." });

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách báo cáo của Citizen đang đăng nhập.
        /// </summary>
        /// <param name="status">Lọc theo trạng thái: Pending, Accepted, Assigned, Collected (bỏ trống = lấy tất cả)</param>
        [HttpGet("my-reports")]
        [ProducesResponseType(typeof(IEnumerable<WasteReportResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyReports([FromQuery] ReportStatus? status = null)
        {
            var citizenId = GetCurrentCitizenId();
            var results = await _wasteReportService.GetReportsByCitizenAsync(citizenId, status);
            return Ok(results);
        }

        /// <summary>
        /// Cập nhật trạng thái báo cáo theo luồng: Pending → Accepted → Assigned → Collected.
        /// </summary>
        /// <param name="id">ID của báo cáo</param>
        /// <param name="dto">Trạng thái mới</param>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(WasteReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReportStatusDto dto)
        {
            // TODO: Kiểm tra role — chỉ Enterprise/Collector mới được đổi status
            var result = await _wasteReportService.UpdateStatusAsync(id, dto);
            return Ok(result);
        }

        // Tạm thời hardcode, sẽ thay bằng JWT claim sau
        private static int GetCurrentCitizenId() => 1;
    }
}
