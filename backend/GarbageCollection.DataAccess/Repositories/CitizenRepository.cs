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

        public async Task<Citizen?> GetByEmailAsync(string email)
        {
            return await _context.Citizens
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task AddAsync(Citizen citizen)
        {
            await _context.Citizens.AddAsync(citizen);
            await _context.SaveChangesAsync();
        }
    }
}