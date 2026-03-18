using Enrich.BLL.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Enrich.BLL.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> UpdateProfileAsync(string userId, UpdateProfileDTO profileDto);
    }
}
