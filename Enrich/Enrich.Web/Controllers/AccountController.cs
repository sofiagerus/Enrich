using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    public class AccountController(
        ILogger<AccountController> logger,
        IAuthService authService,
        IUserService userService) : BaseController
    {
        [HttpGet]
        public IActionResult Signup()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Settings");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning(
                    "Registration form validation failed for Username: {Username}, Email: {Email}.",
                    model.Username,
                    model.Email);

                return View(model);
            }

            var userDto = new UserSignupDTO
            {
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
            };

            var result = await authService.RegisterUserAsync(userDto);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Successfully registered new user Username: {Username}, Email: {Email}",
                    model.Username,
                    model.Email);

                var loginDto = new LoginDTO
                {
                    Email = userDto.Email,
                    Password = userDto.Password,
                    RememberMe = true,
                };

                await authService.LoginAsync(loginDto);
                return RedirectToAction("Settings", "Account");
            }

            logger.LogWarning(
                "Error registering Username: {Username}, Email: {Email}. Error: {Error}",
                model.Username,
                model.Email,
                result.ErrorMessage);

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed.");

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Settings");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning(
                    "Login form validation failed for Email: {Email}.",
                    model.Email);

                return View(model);
            }

            var loginDto = new LoginDTO
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var result = await authService.LoginAsync(loginDto);

            if (result.IsSuccess)
            {
                logger.LogInformation("User with Email: {Email} successfully logged in.", model.Email);
                return RedirectToAction("Settings", "Account");
            }

            logger.LogWarning("Failed login attempt for Email: {Email}. Error: {Error}", model.Email, result.ErrorMessage);
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = CurrentUserId;
            await authService.LogoutAsync();
            logger.LogInformation("User {UserId} logged out.", userId);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings(string tab = "profile")
        {
            var profileDto = await userService.GetCurrentUserProfileAsync(User);

            if (profileDto == null)
            {
                logger.LogWarning("Settings access attempt by unauthorized user.");
                return NotFound("User not found.");
            }

            var model = new UpdateProfileViewModel
            {
                Email = profileDto.Email,
                Username = profileDto.Username,
                Bio = profileDto.Bio
            };

            ViewBag.ActiveTab = tab;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Settings(UpdateProfileViewModel model, string tab = "profile")
        {
            var profileDto = await userService.GetCurrentUserProfileAsync(User);
            if (profileDto == null)
            {
                return NotFound();
            }

            model.Email = profileDto.Email;

            if (!ModelState.IsValid)
            {
                ViewBag.ActiveTab = tab;
                return View(model);
            }

            var updateDto = new UpdateProfileDTO { Username = model.Username, Bio = model.Bio };
            var result = await userService.UpdateProfileAsync(profileDto.Id, updateDto);

            if (result.IsSuccess)
            {
                logger.LogInformation("User {UserId} successfully updated profile.", profileDto.Id);
                TempData["SuccessMessage"] = "Your profile has been successfully updated.";
                return RedirectToAction("Settings", new { tab });
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Update failed.");

            ViewBag.ActiveTab = tab;
            return View(model);
        }
    }
}