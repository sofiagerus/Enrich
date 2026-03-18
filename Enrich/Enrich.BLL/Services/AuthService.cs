using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Enrich.BLL.Services
{
    public class AuthService(UserManager<User> userManager) : IAuthService
    {
        public async Task<IdentityResult> RegisterUserAsync(UserSignupDTO dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            return await userManager.CreateAsync(user, dto.Password);
        }
    }
}