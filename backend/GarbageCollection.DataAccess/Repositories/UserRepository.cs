using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Data;
using GarbageCollection.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => _db.Users
                  .AsNoTracking()
                  .FirstOrDefaultAsync(u => u.Email == email, ct);
        public Task<User?> GetByEmailTrackedAsync(string email, CancellationToken ct = default)
         => _db.Users
               .FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<EmailOtp?> GetLatestByEmailAsync(string email, CancellationToken ct = default)
            => _db.EmailOtps
                  .AsNoTracking()
                  .Where(o => o.Email == email)
                  .OrderByDescending(o => o.CreatedAt)
                  .FirstOrDefaultAsync(ct);

        public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default)
            => _db.Users
                  .AsNoTracking()
                  .FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            await _db.Users.AddAsync(user, ct);
            return user;
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
