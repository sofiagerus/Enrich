namespace Enrich.BLL.Settings
{
    public class PaginationSettings
    {
        public const string Section = "Pagination";

        public int DefaultPersonalWordsPageSize { get; set; } = 20;

        public int DefaultSystemWordsPageSize { get; set; } = 20;

        public int DefaultUserBundlesPageSize { get; set; } = 6;

        public int DefaultSystemBundlesPageSize { get; set; } = 12;
    }
}
