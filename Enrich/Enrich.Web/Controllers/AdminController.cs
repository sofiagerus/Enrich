using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.Web.ViewModels;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminController(
        IUserService userService,
        IValidator<RestrictAccountViewModel> restrictValidator,
        ILogger<AdminController> logger) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            logger.LogInformation("Адміністратор переглядає список усіх користувачів.");

            var usersDto = await userService.GetAllUsersAsync();
            var model = usersDto.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = "User"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Restrict(string id, string username)
        {
            var model = new RestrictAccountViewModel
            {
                UserId = id,
                Username = username
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restrict(RestrictAccountViewModel model)
        {
            var validationResult = await restrictValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return View(model);
            }

            var dto = new RestrictAccountDTO
            {
                UserId = model.UserId,
                Reason = model.Reason
            };

            var result = await userService.RestrictUserAsync(dto);

            if (result.Succeeded)
            {
                logger.LogInformation("Адміністратор заблокував акаунт {UserId}", model.UserId);
                TempData["SuccessMessage"] = $"Акаунт користувача {model.Username} успішно заблоковано.";
                return RedirectToAction("Browse");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}