namespace Enrich.BLL.DTOs
{
    public class RestrictAccountDTO
    {
        public string UserId { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public int LockoutDays { get; set; } = 36500;
    }
}
