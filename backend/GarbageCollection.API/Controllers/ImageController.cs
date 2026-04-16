using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public ImageController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Upload tối đa 3 ảnh lên Cloudinary, trả về danh sách URL.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] IList<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest(new { message = "Vui lòng chọn ít nhất 1 ảnh." });

            if (images.Count > 3)
                return BadRequest(new { message = "Tối đa 3 ảnh mỗi lần upload." });

            var urls = await _cloudinaryService.UploadImagesAsync(images, "waste-reports");
            return Ok(new { imageUrls = urls });
        }
    }
}
