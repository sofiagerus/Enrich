using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}