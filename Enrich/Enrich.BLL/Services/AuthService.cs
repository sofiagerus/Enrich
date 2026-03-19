using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Enrich.BLL.Services
{
    public class AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager) : IAuthService
    {
        public async Task<IdentityResult> RegisterUserAsync(UserSignupDTO dto)
        {
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            return await userManager.CreateAsync(user, dto.Password);
        }

        public async Task<SignInResult> LoginAsync(LoginDTO dto)
        {
            return await signInManager.PasswordSignInAsync(
                dto.Email,
                dto.Password,
                dto.RememberMe,
                lockoutOnFailure: false);
        }

        public async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }
    }
}