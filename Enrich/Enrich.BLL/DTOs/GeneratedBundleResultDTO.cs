using System.Collections.Generic;

namespace Enrich.BLL.DTOs
{
    public class GeneratedBundleResultDTO
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public List<SystemWordDTO> Words { get; set; } = new();
    }
}
