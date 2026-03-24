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
    }
}
