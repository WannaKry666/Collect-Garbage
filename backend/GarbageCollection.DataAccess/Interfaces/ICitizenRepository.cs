using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICitizenRepository
    {
        Task<Citizen?> GetByEmailAsync(string email);
        Task AddAsync(Citizen citizen);
    }
}