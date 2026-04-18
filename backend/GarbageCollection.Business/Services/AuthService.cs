using GarbageCollection.Business.Interfaces;
using GarbageCollection.Common.DTOs;
using GarbageCollection.Common.Models;
using GarbageCollection.DataAccess.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace GarbageCollection.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICitizenRepository _citizenRepository;

        public AuthService(ICitizenRepository citizenRepository)
        {
            _citizenRepository = citizenRepository;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            // 1. Check email tồn tại
            var existing = await _citizenRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
            {
                return "Email already exists";
            }

            // 2. Tạo user
            var citizen = new Citizen
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password)
            };

            // 3. Lưu DB
            await _citizenRepository.AddAsync(citizen);

            return "Register success";
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}