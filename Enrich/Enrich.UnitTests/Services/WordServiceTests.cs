using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class WordServiceTests
    {
        private ApplicationDbContext _dbContext = null!;
        private Mock<ILogger<WordService>> _loggerMock = null!;
        private WordService _wordService = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<WordService>>();
            _wordService = new WordService(_dbContext, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task CreatePersonalWordAsync_WithValidData_CreatesWordAndUserWord()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO
            {
                Term = "Serendipity",
                Translation = "Щасливий випадок",
                DifficultyLevel = "B2",
                PartOfSpeech = "noun",
                Transcription = "/ˌserənˈdɪpɪti/",
                Meaning = "The occurrence of events by chance in a happy way.",
                Example = "It was pure serendipity that we met.",
            };

            // Act
            var (success, error) = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);

            var word = await _dbContext.Words.FirstOrDefaultAsync(w => w.Term == "Serendipity");
            Assert.That(word, Is.Not.Null);
            Assert.That(word!.CreatorId, Is.EqualTo(userId));
            Assert.That(word.IsGlobal, Is.False);
            Assert.That(word.Translation, Is.EqualTo("Щасливий випадок"));

            var userWord = await _dbContext.UserWords.FirstOrDefaultAsync(uw => uw.UserId == userId);
            Assert.That(userWord, Is.Not.Null);
            Assert.That(userWord!.WordId, Is.EqualTo(word.Id));
        }

        [Test]
        public async Task CreatePersonalWordAsync_WithDuplicateTerm_ReturnsFailure()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "Benevolent" };

            await _wordService.CreatePersonalWordAsync(userId, dto);

            // Act – same term, same user
            var (success, error) = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(success, Is.False);
            Assert.That(error, Is.Not.Null);
            Assert.That(error, Does.Contain("Benevolent"));
        }

        [Test]
        public async Task CreatePersonalWordAsync_DuplicateCheckIsCaseInsensitive()
        {
            // Arrange
            var userId = "user-1";
            await _wordService.CreatePersonalWordAsync(userId, new CreatePersonalWordDTO { Term = "hello" });

            // Act – uppercase variant
            var (success, error) = await _wordService.CreatePersonalWordAsync(userId, new CreatePersonalWordDTO { Term = "HELLO" });

            // Assert
            Assert.That(success, Is.False);
            Assert.That(error, Is.Not.Null);
        }

        [Test]
        public async Task CreatePersonalWordAsync_DifferentUser_SameTermAllowed()
        {
            // Arrange
            var dto = new CreatePersonalWordDTO { Term = "Ephemeral" };
            await _wordService.CreatePersonalWordAsync("user-1", dto);

            // Act – different user, same term
            var (success, error) = await _wordService.CreatePersonalWordAsync("user-2", dto);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);

            var words = await _dbContext.Words.Where(w => w.Term == "Ephemeral").ToListAsync();
            Assert.That(words, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task CreatePersonalWordAsync_TrimsWhitespaceFromTerm()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "  Melancholy  " };

            // Act
            var (success, _) = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(success, Is.True);
            var word = await _dbContext.Words.FirstOrDefaultAsync(w => w.Term == "Melancholy");
            Assert.That(word, Is.Not.Null);
        }

        [Test]
        public async Task CreatePersonalWordAsync_WithOnlyRequiredField_Succeeds()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "Ubiquitous" };

            // Act
            var (success, error) = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);

            var word = await _dbContext.Words.FirstOrDefaultAsync(w => w.Term == "Ubiquitous");
            Assert.That(word, Is.Not.Null);
            Assert.That(word!.Translation, Is.Null);
            Assert.That(word.Example, Is.Null);
        }
    }
}
