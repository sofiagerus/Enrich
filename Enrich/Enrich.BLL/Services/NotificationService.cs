using Enrich.BLL.Interfaces;
using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enrich.BLL.Services
{
    public class NotificationService(ApplicationDbContext db) : INotificationService
    {
        public async Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int count = 50)
        {
            return await db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            await db.Notifications
                .Where(n => n.Id == notificationId && n.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task<Notification> CreateAsync(string userId, string type, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync();
            return notification;
        }
    }
}
