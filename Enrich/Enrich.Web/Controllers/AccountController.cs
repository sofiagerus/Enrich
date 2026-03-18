using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.Web.ViewModels;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    public class AccountController(
        ILogger<AccountController> logger,
        IAuthService authService,
        IUserService userService,
        IValidator<SignupViewModel> signupValidator,
        IValidator<UpdateProfileViewModel> profileValidator,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger) : Controller
    {
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            var validationResult = await signupValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors
                    .Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage,
                    })
                    .ToArray();

                logger.LogWarning(
                    "Провалена валідація форми реєстрації для Username: {Username}, Email: {Email}. Деталі: {@ValidationErrors}",
                    model.Username,
                    model.Email,
                    validationErrors);

                validationResult.AddToModelState(ModelState);
                return View(model);
            }

            var userDto = new UserSignupDTO
            {
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
            };

            var result = await authService.RegisterUserAsync(userDto);

            if (result.Succeeded)
            {
                logger.LogInformation(
                    "Успішно зареєстровано нового користувача Username: {Username}, Email: {Email}",
                    model.Username,
                    model.Email);

                var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Profile", "Account");
                }
            }

            var errorCodes = result.Errors.Select(e => e.Code).ToArray();

            logger.LogWarning(
                "Помилка Identity при реєстрації Username: {Username}, Email: {Email}. Коди помилок: {@ErrorCodes}",
                model.Username,
                model.Email,
                errorCodes);

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                logger.LogWarning("Спроба доступу до профілю неавторизованим користувачем.");
                return NotFound("Користувача не знайдено.");
            }

            var model = new UpdateProfileViewModel
            {
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Bio = user.Bio
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            var user = await signInManager.UserManager.GetUserAsync(User);
            if (user == null)
            {
                logger.LogWarning("Спроба оновлення профілю неавторизованим користувачем.");
                return NotFound("Користувача не знайдено.");
            }

            model.Email = user.Email ?? string.Empty;

            var validationResult = await profileValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(model);
            }

            var updateDto = new UpdateProfileDTO
            {
                Username = model.Username,
                Bio = model.Bio
            };

            var result = await userService.UpdateProfileAsync(user.Id, updateDto);

            if (result.Succeeded)
            {
                logger.LogInformation("Користувач {UserId} успішно оновив свій профіль.", user.Id);

                await signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            logger.LogWarning(
                "Користувачу {UserId} не вдалося оновити профіль. Помилки: {Errors}",
                user.Id,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return View(model);
        }
    }
}