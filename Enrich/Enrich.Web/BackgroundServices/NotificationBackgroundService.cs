using Enrich.BLL.Interfaces;
using Enrich.DAL.Data;
using Enrich.DAL.Entities.Enums;
using Enrich.Web.Hubs;
using Enrich.Web.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Enrich.Web.BackgroundServices
{
    public sealed class NotificationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationBackgroundService> logger,
        IOptions<NotificationSettings> options) : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds);

        private readonly Dictionary<string, DateOnly> _streakSentDates = [];

        private static int CalculateStreak(List<DateOnly> sortedDatesDesc, DateOnly referenceDate)
        {
            if (sortedDatesDesc.Count == 0)
            {
                return 0;
            }

            if (sortedDatesDesc[0] < referenceDate.AddDays(-1))
            {
                return 0;
            }

            int streak = 1;
            for (int i = 1; i < sortedDatesDesc.Count; i++)
            {
                if (sortedDatesDesc[i - 1].AddDays(-1) == sortedDatesDesc[i])
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                "NotificationBackgroundService started. Polling every {Interval}s.",
                _interval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendNotificationsAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Unhandled error inside NotificationBackgroundService poll cycle.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            logger.LogInformation("NotificationBackgroundService stopped.");
        }

        private async Task CheckAndSendNotificationsAsync(CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await CheckWordLearnedAsync(db, notificationService, ct);
            await CheckTrainingStreakAsync(db, notificationService, ct);
            await CheckBundleApprovedAsync(db, notificationService, ct);
        }

        private async Task CheckWordLearnedAsync(
            ApplicationDbContext db,
            INotificationService notificationService,
            CancellationToken ct)
        {
            var alreadyNotifiedKeys = await db.Notifications
                .Where(n => n.Type == NotificationTypes.WordLearned)
                .Select(n => n.Message)
                .ToListAsync(ct);

            var learnedProgresses = await db.WordProgresses
                .Include(wp => wp.Word)
                .Where(wp => wp.IsLearned)
                .ToListAsync(ct);

            foreach (var progress in learnedProgresses)
            {
                var key = $"{progress.UserId}|{progress.WordId}";
                if (alreadyNotifiedKeys.Contains(key))
                {
                    continue;
                }

                var notification = await notificationService.CreateAsync(
                    progress.UserId, NotificationTypes.WordLearned, key);

                var displayMessage = $"Congratulations! You have learned the word \"{progress.Word.Term}\"!";

                await SendToUserAsync(progress.UserId, new
                {
                    id = notification.Id,
                    type = notification.Type,
                    message = displayMessage,
                    createdAt = notification.CreatedAt
                });

                logger.LogInformation(
                    "WordLearned notification sent to user {UserId} for word {WordId}.",
                    progress.UserId, progress.WordId);
            }
        }

        private async Task CheckTrainingStreakAsync(
            ApplicationDbContext db,
            INotificationService notificationService,
            CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var userIds = await db.TrainingSessions
                .Where(ts => ts.FinishedAt.HasValue)
                .Select(ts => ts.UserId)
                .Distinct()
                .ToListAsync(ct);

            foreach (var userId in userIds)
            {
                if (_streakSentDates.TryGetValue(userId, out var lastSent) && lastSent == today)
                {
                    continue;
                }

                var sessionDates = await db.TrainingSessions
                    .Where(ts => ts.UserId == userId && ts.FinishedAt.HasValue)
                    .Select(ts => DateOnly.FromDateTime(ts.FinishedAt!.Value))
                    .Distinct()
                    .OrderByDescending(d => d)
                    .Take(10)
                    .ToListAsync(ct);

                int streak = CalculateStreak(sessionDates, today);

                if (streak >= 3)
                {
                    _streakSentDates[userId] = today;

                    var message = $"Amazing! You have trained {streak} days in a row!";
                    var notification = await notificationService.CreateAsync(
                        userId, NotificationTypes.TrainingStreak, message);

                    await SendToUserAsync(userId, new
                    {
                        id = notification.Id,
                        type = notification.Type,
                        message,
                        createdAt = notification.CreatedAt
                    });

                    logger.LogInformation(
                        "TrainingStreak ({Streak} days) notification sent to user {UserId}.",
                        streak, userId);
                }
            }
        }

        private async Task CheckBundleApprovedAsync(
            ApplicationDbContext db,
            INotificationService notificationService,
            CancellationToken ct)
        {
            var alreadyNotifiedBundleIds = await db.Notifications
                .Where(n => n.Type == NotificationTypes.BundleApproved)
                .Select(n => n.Message)
                .ToListAsync(ct);

            var approvedBundles = await db.Bundles
                .Where(b => b.Status == BundleStatus.Published
                         && b.OwnerId != null
                         && !b.IsSystem)
                .ToListAsync(ct);

            foreach (var bundle in approvedBundles)
            {
                var key = bundle.Id.ToString();
                if (alreadyNotifiedBundleIds.Contains(key))
                {
                    continue;
                }

                var ownerId = bundle.OwnerId!;
                var displayMessage = $"Your collection \"{bundle.Title}\" has been approved and published!";

                var notification = await notificationService.CreateAsync(
                    ownerId, NotificationTypes.BundleApproved, key);

                await SendToUserAsync(ownerId, new
                {
                    id = notification.Id,
                    type = notification.Type,
                    message = displayMessage,
                    createdAt = notification.CreatedAt
                });

                logger.LogInformation(
                    "BundleApproved notification sent to user {UserId} for bundle {BundleId} (\"{Title}\").",
                    ownerId, bundle.Id, bundle.Title);
            }
        }

        private async Task SendToUserAsync(string userId, object payload)
        {
            await hubContext.Clients
                .Group(userId)
                .SendAsync("ReceiveNotification", payload);
        }
    }
}
