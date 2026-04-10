namespace Enrich.Web.Settings
{
    public class IdentitySettings
    {
        public const string Section = "Identity";

        public bool RequireDigit { get; set; } = true;

        public int RequiredLength { get; set; } = 8;

        public bool RequireNonAlphanumeric { get; set; } = false;

        public bool LockoutAllowedForNewUsers { get; set; } = true;
    }
}
