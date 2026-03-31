using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Repositories
{
    public class BundleRepository(ApplicationDbContext dbContext) : IBundleRepository
    {
        public async Task<(IEnumerable<Bundle> Items, int Total)> GetSystemBundlesPageAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize)
        {
            var query = dbContext.Bundles
                .Where(b => b.IsSystem && b.Status == BundleStatus.Published)
                .Include(b => b.Categories)
                .Include(b => b.Words)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var st = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(st) ||
                                         (b.Description != null && b.Description.ToLower().Contains(st)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var catsLower = category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Select(c => c.ToLower()).ToArray();
                query = query.Where(b => b.Categories.Any(c => catsLower.Contains(c.Name.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(difficultyLevel))
            {
                var levelsLower = difficultyLevel.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                .Select(l => l.ToLower()).ToArray();
                query = query.Where(b => b.DifficultyLevels.Any(dl => levelsLower.Contains(dl.ToLower())));
            }

            if (minWordCount.HasValue)
            {
                query = query.Where(b => b.Words.Count >= minWordCount.Value);
            }

            if (maxWordCount.HasValue)
            {
                query = query.Where(b => b.Words.Count <= maxWordCount.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Bundle?> GetBundleAsync(int bundleId)
        {
            return await dbContext.Bundles
                .Include(b => b.Categories)
                .Include(b => b.Words)
                .FirstOrDefaultAsync(b => b.Id == bundleId);
        }

        public async Task<Bundle> CreateBundleAsync(Bundle bundle)
        {
            dbContext.Bundles.Add(bundle);
            await dbContext.SaveChangesAsync();
            return bundle;
        }
    }
}
