using Microsoft.AspNetCore.Mvc;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.Business.Interfaces;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Lấy thông tin profile của citizen đang đăng nhập.
        /// </summary>
        [HttpGet("/api/v1/users/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var citizenId = GetCurrentCitizenId();
            var result = await _userService.GetProfileAsync(citizenId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result, "get user profile successfully"));
        }

        // TODO: thay bằng JWT claim sau khi có auth
        private static int GetCurrentCitizenId() => 1;
    }
}
