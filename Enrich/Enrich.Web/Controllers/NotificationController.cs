using Enrich.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var notifications = await notificationService.GetUserNotificationsAsync(CurrentUserId);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await notificationService.MarkAllAsReadAsync(CurrentUserId);
            logger.LogInformation("User {UserId} marked all notifications as read.", CurrentUserId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await notificationService.MarkAsReadAsync(id, CurrentUserId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await notificationService.GetUnreadCountAsync(CurrentUserId);
            return Json(new { count });
        }
    }
}
