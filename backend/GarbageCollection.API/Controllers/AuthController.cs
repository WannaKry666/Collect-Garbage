using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            // Nếu muốn đơn giản (đủ demo)
            return Ok(new
            {
                message = result
            });
        }
    }
}