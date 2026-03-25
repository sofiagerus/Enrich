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
            await dbContext.SaveChangesAsync();

            userWord.WordId = word.Id;
            dbContext.UserWords.Add(userWord);
            await dbContext.SaveChangesAsync();

            return word;
        }

        public async Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId)
        {
            var userWords = await dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                .ToListAsync();

            return userWords;
        }

        public IQueryable<UserWord> QueryPersonalWords(string userId)
        {
            return dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                .AsQueryable();
        }

        public async Task<(IEnumerable<UserWord> Items, int Total)> GetPersonalWordsPageAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize)
        {
            var query = dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var st = searchTerm.Trim().ToLower();
                query = query.Where(uw => uw.Word.Term.ToLower().Contains(st) || (!string.IsNullOrEmpty(uw.Word.Translation) && uw.Word.Translation.ToLower().Contains(st)));
            }

            if (!string.IsNullOrWhiteSpace(partOfSpeech))
            {
                query = query.Where(uw => uw.Word.PartOfSpeech == partOfSpeech);
            }

            if (!string.IsNullOrWhiteSpace(difficultyLevel))
            {
                query = query.Where(uw => uw.Word.DifficultyLevel == difficultyLevel);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(uw => uw.Word.Categories.Any(c => c.Name == category));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(uw => uw.Word.Term)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<UserWord?> GetUserWordAsync(string userId, int wordId)
        {
            return await dbContext.UserWords
                .Include(uw => uw.Word)
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
    }
}
