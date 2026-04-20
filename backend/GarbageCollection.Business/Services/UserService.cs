using GarbageCollection.Business.Helpers;
using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class UserService : IUserService
    {
        private readonly ICitizenRepository _citizenRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;

        public UserService(
            ICitizenRepository citizenRepository,
            IUserRepository userRepository,
            JwtHelper jwtHelper)
        {
            _citizenRepository = citizenRepository;
            _userRepository    = userRepository;
            _jwtHelper         = jwtHelper;
        }

        public async Task<UserProfileDto> GetProfileAsync(int citizenId)
        {
            var citizen = await _citizenRepository.GetByIdAsync(citizenId)
                ?? throw new KeyNotFoundException("account not found");

            return MapToDto(citizen);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int citizenId, UpdateUserProfileData data)
        {
            var citizen = await _citizenRepository.GetByIdAsync(citizenId)
                ?? throw new KeyNotFoundException("account not found");

            citizen.FullName  = data.Fullname;
            citizen.Address   = data.Address;
            citizen.AvatarUrl = data.AvatarUrl;

            var updated = await _citizenRepository.UpdateAsync(citizen);
            return MapToDto(updated);
        }

        public async Task<string> ChangePasswordAsync(string email, ChangePasswordData data, CancellationToken ct = default)
        {
            // Validate new_password format
            var pwError = ValidationHelper.GetPasswordValidationError(data.NewPassword);
            if (pwError != null)
                throw new ArgumentException(pwError);

            var user = await _userRepository.GetByEmailTrackedAsync(email, ct)
                ?? throw new KeyNotFoundException("account not found");

            if (user.IsBanned)
                throw new UnauthorizedAccessException("forbidden");

            // Verify old password
            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !PasswordHelper.Verify(data.OldPassword, user.PasswordHash))
                throw new ArgumentException("old password is incorrect");

            // New password must differ from old
            if (PasswordHelper.Verify(data.NewPassword, user.PasswordHash))
                throw new ArgumentException("new password must be different from old password");

            // Hash and save new password
            user.PasswordHash = PasswordHelper.Hash(data.NewPassword);
            user.UpdatedAt    = DateTime.UtcNow;

            if (data.LogoutAllDevices)
                user.LoginTerm++;

            await _userRepository.SaveChangesAsync(ct);

            // Generate new access token
            var (token, _) = _jwtHelper.GenerateAccessToken(user.Email, user.FullName, user.LoginTerm);
            return token;
        }

        private static UserProfileDto MapToDto(GarbageCollection.Common.Models.Citizen c) => new()
        {
            Email     = c.Email,
            Fullname  = c.FullName,
            Address   = c.Address,
            AvatarUrl = c.AvatarUrl,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
