using Enrich.BLL.Interfaces;
using Enrich.Web.ViewModels;

// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminController(
        IUserService userService,
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
    }
}