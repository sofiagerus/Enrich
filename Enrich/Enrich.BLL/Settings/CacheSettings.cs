namespace Enrich.BLL.Settings
{
    public class CacheSettings
    {
        public const string Section = "CacheSettings";

        public int CategoriesCacheDurationMinutes { get; set; } = 60;
    }
}
