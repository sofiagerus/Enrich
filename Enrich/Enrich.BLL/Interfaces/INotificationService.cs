using Enrich.DAL.Entities;

namespace Enrich.BLL.Interfaces
{
    public interface INotificationService
    {
        Task<IReadOnlyList<Notification>> GetUserNotificationsAsync(string userId, int count = 50);

        Task<int> GetUnreadCountAsync(string userId);

        Task MarkAllAsReadAsync(string userId);

        Task MarkAsReadAsync(int notificationId, string userId);

        Task<Notification> CreateAsync(string userId, string type, string message);
    }
}
