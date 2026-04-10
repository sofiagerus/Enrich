using Enrich.BLL.DTOs;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class SystemWordsIndexViewModel
    {
        public PagedResult<SystemWordDTO> Words { get; set; } = new PagedResult<SystemWordDTO>();

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public string? SearchTerm { get; set; }

        public string? CategoryFilter { get; set; }

        public string? LevelFilter { get; set; }

        public string? PosFilter { get; set; }

        public int PageSize { get; set; } = 12;
    }
}
