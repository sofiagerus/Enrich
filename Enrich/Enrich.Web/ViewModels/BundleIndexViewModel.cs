using Enrich.BLL.DTOs;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class BundleIndexViewModel
    {
        public PagedResult<BundleDTO> Bundles { get; set; } = new();

        public string? SearchTerm { get; set; }

        public string? LevelFilter { get; set; }

        public int? MinWordCount { get; set; }

        public int? MaxWordCount { get; set; }

        public IEnumerable<Category> Categories { get; set; } = [];
    }
}