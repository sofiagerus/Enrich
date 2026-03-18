using Enrich.BLL.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Enrich.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(UserSignupDTO dto);
    }
}