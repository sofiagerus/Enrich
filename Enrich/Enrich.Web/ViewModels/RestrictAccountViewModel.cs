namespace Enrich.Web.ViewModels
{
    public class RestrictAccountViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty; // Для відображення в UI

        public string Reason { get; set; } = string.Empty;
    }
}