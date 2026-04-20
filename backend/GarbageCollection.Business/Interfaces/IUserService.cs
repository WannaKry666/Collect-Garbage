using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int citizenId);
        Task<UserProfileDto> UpdateProfileAsync(int citizenId, UpdateUserProfileData data);

        /// <summary>
        /// Đổi mật khẩu. Trả về accessToken mới để controller set cookie.
        /// </summary>
        Task<string> ChangePasswordAsync(string email, ChangePasswordData data, CancellationToken ct = default);
    }
}
