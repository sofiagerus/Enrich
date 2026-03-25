using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Repositories
{
    public class WordRepository(ApplicationDbContext dbContext) : IWordRepository
    {
        public async Task<bool> WordExistsForUserAsync(string userId, string termLower)
        {
            return await dbContext.Words
                .AnyAsync(w =>
                    w.CreatorId == userId &&
                    w.Term.ToLower() == termLower);
        }

        public async Task<Word> CreatePersonalWordAsync(Word word, UserWord userWord)
        {
            dbContext.Words.Add(word);

            userWord.Word = word;
            dbContext.UserWords.Add(userWord);

            await dbContext.SaveChangesAsync();

            return word;
        }

        public async Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId)
        {
            return await dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                    .ThenInclude(w => w.Categories)
                .ToListAsync();
        }

        public IQueryable<UserWord> QueryPersonalWords(string userId)
        {
            return dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                    .ThenInclude(w => w.Categories)
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<(IEnumerable<UserWord> Items, int Total)> GetPersonalWordsPageAsync(
            string userId,
            string? searchTerm,
            string? category,
            string? partOfSpeech,
            string? difficultyLevel,
            int page,
            int pageSize)
        {
            var query = dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                    .ThenInclude(w => w.Categories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var st = searchTerm.Trim().ToLower();
                query = query.Where(uw => uw.Word.Term.ToLower().Contains(st) ||
                                         (uw.Word.Translation != null && uw.Word.Translation.ToLower().Contains(st)));
            }

            if (!string.IsNullOrWhiteSpace(partOfSpeech))
            {
                var partsLower = partOfSpeech.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                            .Select(p => p.ToLower()).ToArray();
                query = query.Where(uw => uw.Word.PartOfSpeech != null && partsLower.Contains(uw.Word.PartOfSpeech.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(difficultyLevel))
            {
                var levelsLower = difficultyLevel.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                               .Select(p => p.ToLower()).ToArray();
                query = query.Where(uw => uw.Word.DifficultyLevel != null && levelsLower.Contains(uw.Word.DifficultyLevel.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var catsLower = category.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Select(p => p.ToLower()).ToArray();
                query = query.Where(uw => uw.Word.Categories.Any(c => catsLower.Contains(c.Name.ToLower())));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(uw => uw.SavedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<UserWord?> GetUserWordAsync(string userId, int wordId)
        {
            return await dbContext.UserWords
                .Include(uw => uw.Word)
                    .ThenInclude(w => w.Categories)
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);
        }

        public async Task DeleteUserWordAsync(UserWord userWord)
        {
            dbContext.UserWords.Remove(userWord);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteWordAsync(Word word)
        {
            dbContext.Words.Remove(word);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await dbContext.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var n = name.Trim().ToLower();
            return await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == n);
        }

        public async Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null)
            {
                return [];
            }

            var idArray = ids.Where(i => i > 0).Distinct().ToArray();

            return await dbContext.Categories
                .Where(c => idArray.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}