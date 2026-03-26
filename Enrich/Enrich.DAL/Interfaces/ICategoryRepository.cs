using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids);

        Task<Category> CreateCategoryAsync(Category category);

        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
