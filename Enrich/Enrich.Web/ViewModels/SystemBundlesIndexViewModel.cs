using Enrich.BLL.DTOs;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class SystemBundlesIndexViewModel
    {
        public PagedResult<SystemBundleDTO> Bundles { get; set; } = new PagedResult<SystemBundleDTO>();

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? SearchTerm { get; set; }

        public string? CategoryFilter { get; set; }

        public string? LevelFilter { get; set; }

        public int? MinWordCount { get; set; }

        public int? MaxWordCount { get; set; }

        public int PageSize { get; set; } = 12;
    }
}
