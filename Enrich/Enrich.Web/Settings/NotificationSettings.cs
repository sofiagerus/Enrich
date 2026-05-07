namespace Enrich.Web.Settings
{
    public class NotificationSettings
    {
        public const string Section = "NotificationService";

        public int IntervalSeconds { get; set; } = 30;
    }
}
