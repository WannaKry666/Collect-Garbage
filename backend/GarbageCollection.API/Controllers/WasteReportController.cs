using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs.WasteReport;
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
        /// Lấy toàn bộ báo cáo của Citizen đang đăng nhập.
        /// </summary>
        [HttpGet("my-reports")]
        [ProducesResponseType(typeof(IEnumerable<WasteReportResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyReports()
        {
            var citizenId = GetCurrentCitizenId();
            var results = await _wasteReportService.GetReportsByCitizenAsync(citizenId);
            return Ok(results);
        }

        // Tạm thời hardcode, sẽ thay bằng JWT claim sau
        private static int GetCurrentCitizenId() => 1;
    }
}
