using GarbageCollection.Common.Models;

namespace GarbageCollection.DataAccess.Interfaces
{
    public interface ICitizenRepository
    {
        Task<Citizen?> GetByIdAsync(int id);
    }
}
