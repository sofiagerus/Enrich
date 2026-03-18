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
        IAuthService authService,
        IValidator<SignupViewModel> validator,
        SignInManager<User> signInManager) : Controller
    {
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            var validationResult = await validator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(model);
            }

            var userDto = new UserSignupDTO
            {
                Email = model.Email,
                Username = model.Username,
                Password = model.Password
            };

            var result = await authService.RegisterUserAsync(userDto);

            if (result.Succeeded)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Profile", "Account");
                }
            }

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

            return user == null ? NotFound("Користувача не знайдено.") : View(user);
        }
    }
}