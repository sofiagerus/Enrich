using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Repositories
{
    public class BundleRepository(ApplicationDbContext dbContext) : IBundleRepository
    {
        public async Task<Bundle> CreateBundleAsync(Bundle bundle)
        {
            dbContext.Bundles.Add(bundle);
            await dbContext.SaveChangesAsync();
            return bundle;
        }

        public async Task<Bundle?> GetBundleByIdAsync(int bundleId)
        {
            return await dbContext.Bundles.FindAsync(bundleId);
        }

        public async Task<Bundle?> GetBundleByIdWithDetailsAsync(int bundleId)
        {
            return await dbContext.Bundles
                .Include(b => b.Owner)
                .Include(b => b.Words)
                .Include(b => b.Categories)
                .Include(b => b.Tags)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == bundleId);
        }

        public async Task<IEnumerable<Bundle>> GetUserBundlesAsync(string userId)
        {
            return await dbContext.Bundles
                .Where(b => b.OwnerId == userId)
                .Include(b => b.Words)
                .Include(b => b.Categories)
                .Include(b => b.Tags)
                .AsSplitQuery()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Bundle> Items, int Total)> GetUserBundlesPageAsync(
            string userId,
            string? searchTerm,
            int page,
            int pageSize)
        {
            var query = dbContext.Bundles
                .Where(b => b.OwnerId == userId)
                .Include(b => b.Words)
                .Include(b => b.Categories)
                .Include(b => b.Tags)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var st = searchTerm.Trim().ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(st) ||
                                         (b.Description != null && b.Description.ToLower().Contains(st)));
            }

            var total = await query.CountAsync();

            var bundles = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bundles, total);
        }

        public async Task UpdateBundleAsync(Bundle bundle)
        {
            dbContext.Bundles.Update(bundle);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteBundleAsync(int bundleId)
        {
            var bundle = await dbContext.Bundles
                .Include(b => b.Words)
                .Include(b => b.Categories)
                .Include(b => b.Tags)
                .Include(b => b.UserBundles)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == bundleId);

            if (bundle != null)
            {
                bundle.Words.Clear();
                bundle.Categories.Clear();
                bundle.Tags.Clear();
                bundle.UserBundles.Clear();

                dbContext.Bundles.Remove(bundle);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> BundleTitleExistsForUserAsync(string userId, string titleLower)
        {
            return await dbContext.Bundles
                .AnyAsync(b =>
                    b.OwnerId == userId &&
                    b.Title.ToLower() == titleLower);
        }

        public async Task AddWordsToBundleAsync(int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await dbContext.Bundles
                .Include(b => b.Words)
                .FirstOrDefaultAsync(b => b.Id == bundleId);

            if (bundle != null)
            {
                var words = await dbContext.Words
                    .Where(w => wordIds.Contains(w.Id))
                    .ToListAsync();

                foreach (var word in words)
                {
                    if (!bundle.Words.Any(w => w.Id == word.Id))
                    {
                        bundle.Words.Add(word);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveWordsFromBundleAsync(int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await dbContext.Bundles
                .Include(b => b.Words)
                .FirstOrDefaultAsync(b => b.Id == bundleId);

            if (bundle != null)
            {
                var wordsToRemove = bundle.Words.Where(w => wordIds.Contains(w.Id)).ToList();

                foreach (var word in wordsToRemove)
                {
                    bundle.Words.Remove(word);
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task AddCategoriesToBundleAsync(int bundleId, IEnumerable<int> categoryIds)
        {
            var bundle = await dbContext.Bundles
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == bundleId);

            if (bundle != null)
            {
                var categories = await dbContext.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var category in categories)
                {
                    if (!bundle.Categories.Any(c => c.Id == category.Id))
                    {
                        bundle.Categories.Add(category);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task AddTagsToBundleAsync(int bundleId, IEnumerable<int> tagIds)
        {
            var bundle = await dbContext.Bundles
                .Include(b => b.Tags)
                .FirstOrDefaultAsync(b => b.Id == bundleId);

            if (bundle != null)
            {
                var tags = await dbContext.Tags
                    .Where(t => tagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in tags)
                {
                    if (!bundle.Tags.Any(t => t.Id == tag.Id))
                    {
                        bundle.Tags.Add(tag);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
