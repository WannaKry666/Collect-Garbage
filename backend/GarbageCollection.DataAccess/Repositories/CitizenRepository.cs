using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GarbageCollection.DataAccess.Repositories
{
    public class CitizenRepository : ICitizenRepository
    {
        private readonly AppDbContext _context;

        public CitizenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Citizen?> GetByIdAsync(int id)
            => await _context.Citizens.FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Citizen?> GetByEmailAsync(string email)
            => await _context.Citizens.FirstOrDefaultAsync(c => c.Email == email);

        public async Task<Citizen> UpdateAsync(Citizen citizen)
        {
            citizen.UpdatedAt = DateTime.UtcNow;
            _context.Citizens.Update(citizen);
            await _context.SaveChangesAsync();
            return citizen;
        }
    }
}
