using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class UserService(
        UserManager<User> userManager,
        ILogger<UserService> logger) : IUserService
    {
        public async Task<IdentityResult> UpdateProfileAsync(string userId, UpdateProfileDTO profileDto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("Спроба оновити неіснуючого користувача з ID: {UserId}", userId);
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено." });
            }

            user.Bio = profileDto.Bio;
            user.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (user.UserName != profileDto.Username)
            {
                var setUsernameResult = await userManager.SetUserNameAsync(user, profileDto.Username);

                if (!setUsernameResult.Succeeded)
                {
                    logger.LogError(
                        "Не вдалося змінити Username для {UserId}: {Errors}",
                        userId,
                        string.Join(", ", setUsernameResult.Errors.Select(e => e.Description)));

                    return setUsernameResult;
                }

                await userManager.UpdateNormalizedUserNameAsync(user);
            }

            var updateResult = await userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                logger.LogInformation("Профіль користувача {UserId} успішно оновлено.", userId);
            }
            else
            {
                logger.LogError(
                    "Помилка при фінальному збереженні користувача {UserId}: {Errors}",
                    userId,
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            }

            return updateResult;
        }
    }
}