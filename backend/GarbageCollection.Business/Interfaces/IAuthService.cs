using GarbageCollection.Common.DTOs;

namespace GarbageCollection.Business.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
    }
}