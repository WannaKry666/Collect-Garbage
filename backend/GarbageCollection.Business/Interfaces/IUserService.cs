using GarbageCollection.Common.DTOs.User;

namespace GarbageCollection.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int citizenId);
    }
}
