using System.Text.Json;
using Enrich.BLL.DTOs;

namespace Enrich.Web.ViewModels
{
    public class PreviewGeneratedViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string WordsJson { get; set; } = "[]";

        public List<SystemWordDTO> Words => string.IsNullOrWhiteSpace(WordsJson)
            ? new List<SystemWordDTO>()
            : JsonSerializer.Deserialize<List<SystemWordDTO>>(WordsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SystemWordDTO>();
    }
}