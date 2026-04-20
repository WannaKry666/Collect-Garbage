using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs.User;
using GarbageCollection.DataAccess.Interfaces;

namespace GarbageCollection.Business.Services
{
    public class UserService : IUserService
    {
        private readonly ICitizenRepository _citizenRepository;

        public UserService(ICitizenRepository citizenRepository)
        {
            _citizenRepository = citizenRepository;
        }

        public async Task<UserProfileDto> GetProfileAsync(int citizenId)
        {
            var citizen = await _citizenRepository.GetByIdAsync(citizenId)
                ?? throw new KeyNotFoundException("account not found");

            return new UserProfileDto
            {
                Email     = citizen.Email,
                Fullname  = citizen.FullName,
                Address   = citizen.Address,
                AvatarUrl = citizen.AvatarUrl
            };
        }
    }
}
