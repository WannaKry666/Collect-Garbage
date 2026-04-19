using GarbageCollection.API.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Business.Services;
using GarbageCollection.Common.DTOs.Auth;
using GarbageCollection.Common.DTOs.Auth.Local;
using GarbageCollection.Common.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;

namespace GarbageCollection.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILocalAuthService _localAuthService;

        public AuthController(IAuthService authService, IConfiguration configuration, ILocalAuthService localAuthService)
        {
            _authService = authService;
            _configuration = configuration;
            _localAuthService = localAuthService;
        }

        /// <summary>
        /// POST /api/v1/auth/google-auth/account
        /// Validates a Google ID token and returns user profile + auth cookies.
        /// </summary>
        [HttpPost("google-auth/account")]
        [ProducesResponseType(typeof(ApiResponse<GoogleLoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GoogleLogin(
            [FromBody] GoogleLoginRequestWrapper request,
            CancellationToken ct)
        {
            if (request?.Data?.GoogleToken is null or { Length: 0 })
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "google_token is required",
                    "VALIDATION_ERROR",
                    "The google_token field must not be empty."));
            }

            var result = await _authService.GoogleLoginAsync(request.Data.GoogleToken, ct);

            if (!result.Succeeded)
            {
                var failBody = ApiResponse<object>.Fail(
                    result.FailMessage!,
                    result.FailCode!,
                    result.FailDescription!);

                return StatusCode(result.HttpStatusCode, failBody);
            }

            // STEP 5 – set HttpOnly cookies (controller concern, not service concern)
            CookieHelper.SetAuthCookies(Response, result.AccessToken!, result.RefreshToken!, _configuration);

            return Ok(ApiResponse<GoogleLoginResponseDto>.Success(
                "account is valid",
                result.Payload!));
        }
    
     // ───────────────── REGISTER LOCAL ─────────────────
        [HttpPost("local-auth/account-registration")]
        public async Task<IActionResult> Register(
            [FromBody] LocalRegisterRequestWrapper request)
        {
            var result = await _localAuthService.RegisterAsync(request.Data);

            if (!result.Succeeded)
            {
                return StatusCode(result.HttpStatusCode,
                    ApiResponse<object>.Fail(
                        result.FailMessage!,
                        result.FailCode!,
                        result.FailDescription!));
            }

            // 🍪 set cookies
            SetAuthCookies(result.AccessToken!, result.RefreshToken!);

            return Ok(ApiResponse<LocalRegisterResponseDto>.Success(
                "account is created",
                result.Payload!));
        }

        // ───────────────── HELPER ─────────────────
        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15) // match JwtHelper
            });

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}
