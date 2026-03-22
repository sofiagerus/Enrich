using Enrich.BLL.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Enrich.BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        Task<IdentityResult> UpdateProfileAsync(string userId, UpdateProfileDTO profileDto);

        Task<IdentityResult> RestrictUserAsync(RestrictAccountDTO dto);

        Task<UserDTO?> GetCurrentUserProfileAsync(System.Security.Claims.ClaimsPrincipal userPrincipal);

        string? GetCurrentUserId(System.Security.Claims.ClaimsPrincipal userPrincipal);
    }
}
