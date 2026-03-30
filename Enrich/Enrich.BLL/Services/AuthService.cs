using Enrich.BLL.Common;
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
        public async Task<Result> RegisterUserAsync(UserSignupDTO dto)
        {
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var identityResult = await userManager.CreateAsync(user, dto.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return errors;
            }

            return true;
        }

        public async Task<Result> LoginAsync(LoginDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return "Invalid email or password.";
            }

            var signInResult = await signInManager.PasswordSignInAsync(
                user.UserName!,
                dto.Password,
                dto.RememberMe,
                lockoutOnFailure: false);

            if (signInResult.Succeeded)
            {
                return true;
            }

            if (signInResult.IsLockedOut)
            {
                return "Your account is locked. Please contact the administrator.";
            }

            return "Invalid email or password.";
        }

        public async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }
    }
}