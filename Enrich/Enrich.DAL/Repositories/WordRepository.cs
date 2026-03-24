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

        public async Task<Word> CreatePersonalWordAsync(string userId, string term, string? translation, string? transcription,
            string? meaning, string? partOfSpeech, string? example, string? difficultyLevel)
        {
            var word = new Word
            {
                Term = term.Trim(),
                Translation = translation?.Trim(),
                Transcription = transcription?.Trim(),
                Meaning = meaning?.Trim(),
                PartOfSpeech = partOfSpeech?.Trim(),
                Example = example?.Trim(),
                DifficultyLevel = difficultyLevel?.Trim(),
                IsGlobal = false,
                CreatorId = userId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

            dbContext.Words.Add(word);
            await dbContext.SaveChangesAsync();

            var userWord = new UserWord
            {
                UserId = userId,
                WordId = word.Id,
                SavedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

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
