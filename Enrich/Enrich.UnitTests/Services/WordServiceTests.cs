using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class WordServiceTests
    {
        private Mock<IWordRepository> _wordRepositoryMock = null!;
        private Mock<ICategoryRepository> _categoryRepositoryMock = null!;
        private Mock<IWordProgressRepository> _wordProgressRepositoryMock = null!;
        private Mock<ILogger<WordService>> _loggerMock = null!;
        private Mock<IOptions<PaginationSettings>> _paginationOptionsMock = null!;
        private IMemoryCache _memoryCache = null!;
        private WordService _wordService = null!;

        [SetUp]
        public void SetUp()
        {
            _wordRepositoryMock = new Mock<IWordRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _wordProgressRepositoryMock = new Mock<IWordProgressRepository>();
            _loggerMock = new Mock<ILogger<WordService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();
            _paginationOptionsMock.Setup(o => o.Value).Returns(new PaginationSettings());

            var cacheSettings = Options.Create(new CacheSettings { CategoriesCacheDurationMinutes = 60 });

            _wordService = new WordService(
                _wordRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _wordProgressRepositoryMock.Object,
                _memoryCache,
                cacheSettings,
                _paginationOptionsMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache?.Dispose();
        }

        [Test]
        public async Task GetPersonalWordsAsync_WithFilters_ReturnsPagedResult()
        {
            // Arrange
            var userId = "user-1";
            var userWords = new List<UserWord>
            {
                new UserWord { UserId = userId, WordId = 1, Word = new Word { Id = 1, Term = "Apple", PartOfSpeech = "noun" } },
                new UserWord { UserId = userId, WordId = 2, Word = new Word { Id = 2, Term = "Application", PartOfSpeech = "noun" } }
            };

            _wordRepositoryMock
                .Setup(r => r.GetPersonalWordsPageAsync(userId, "app", "Fruits", "noun", null, 1, 10))
                .ReturnsAsync((userWords.AsEnumerable(), 2));

            _wordProgressRepositoryMock
                .Setup(r => r.GetWordProgressesAsync(userId, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<WordProgress>
                {
                    new WordProgress { UserId = userId, WordId = 1, Points = 50 },
                    new WordProgress { UserId = userId, WordId = 2, Points = 100 }
                });

            // Act
            var pageResult = await _wordService.GetPersonalWordsAsync(userId, "app", "Fruits", "noun", null, 1, 10);

            // Assert
            Assert.That(pageResult.TotalCount, Is.EqualTo(2));
            Assert.That(pageResult.Items.Count(), Is.EqualTo(2));
            _wordRepositoryMock.Verify(r => r.GetPersonalWordsPageAsync(userId, "app", "Fruits", "noun", null, 1, 10), Times.Once);
        }

        [Test]
        public async Task DeleteWordAsync_UserRemovesOwnPersonalWord_DeletesWordAndUserWord()
        {
            // Arrange
            var userId = "user-1";
            var word = new Word { Id = 1, Term = "Transient", IsGlobal = false, CreatorId = userId };
            var userWord = new UserWord { UserId = userId, WordId = 1, Word = word };

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(userId, word.Id)).ReturnsAsync(userWord);

            // Act
            var result = await _wordService.DeleteWordAsync(userId, word.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            _wordRepositoryMock.Verify(r => r.DeleteWordAsync(word), Times.Once);
            _wordRepositoryMock.Verify(r => r.DeleteUserWordAsync(It.IsAny<UserWord>()), Times.Never);
        }

        [Test]
        public async Task DeleteWordAsync_UserRemovesSavedSystemWord_DeletesOnlyUserWord()
        {
            // Arrange
            var userId = "user-1";
            var systemWord = new Word { Id = 1, Term = "SystemWord", IsGlobal = true, CreatorId = null };
            var userWord = new UserWord { UserId = userId, WordId = 1, Word = systemWord };

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(userId, systemWord.Id)).ReturnsAsync(userWord);

            // Act
            var result = await _wordService.DeleteWordAsync(userId, systemWord.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            _wordRepositoryMock.Verify(r => r.DeleteUserWordAsync(userWord), Times.Once);
            _wordRepositoryMock.Verify(r => r.DeleteWordAsync(It.IsAny<Word>()), Times.Never);
        }

        [Test]
        public async Task DeleteWordAsync_UserTriesToDeleteNotSavedWord_ReturnsFailure()
        {
            // Arrange
            var userId = "user-1";
            var wordId = 99;

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(userId, wordId)).ReturnsAsync((UserWord?)null);

            // Act
            var result = await _wordService.DeleteWordAsync(userId, wordId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
            _wordRepositoryMock.Verify(r => r.DeleteUserWordAsync(It.IsAny<UserWord>()), Times.Never);
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

            _wordRepositoryMock.Setup(r => r.WordExistsForUserAsync(userId, "serendipity")).ReturnsAsync(false);
            _wordRepositoryMock
                .Setup(r => r.CreatePersonalWordAsync(It.IsAny<Word>(), It.IsAny<UserWord>()))
                .ReturnsAsync((Word w, UserWord uw) => w);

            // Act
            var result = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            _wordRepositoryMock.Verify(
                r => r.CreatePersonalWordAsync(
                It.Is<Word>(w => w.Term == "Serendipity" && w.CreatorId == userId && !w.IsGlobal),
                It.Is<UserWord>(uw => uw.UserId == userId)), Times.Once);
        }

        [Test]
        public async Task CreatePersonalWordAsync_WithDuplicateTerm_ReturnsFailure()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "Benevolent" };

            _wordRepositoryMock.Setup(r => r.WordExistsForUserAsync(userId, "benevolent")).ReturnsAsync(true);

            // Act
            var result = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
            Assert.That(result.ErrorMessage, Does.Contain("Benevolent"));
            _wordRepositoryMock.Verify(r => r.CreatePersonalWordAsync(It.IsAny<Word>(), It.IsAny<UserWord>()), Times.Never);
        }

        [Test]
        public async Task CreatePersonalWordAsync_DuplicateCheckIsCaseInsensitive()
        {
            // Arrange
            var userId = "user-1";

            _wordRepositoryMock.Setup(r => r.WordExistsForUserAsync(userId, "hello")).ReturnsAsync(true);

            // Act
            var result = await _wordService.CreatePersonalWordAsync(userId, new CreatePersonalWordDTO { Term = "HELLO" });

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public async Task CreatePersonalWordAsync_TrimsWhitespaceFromTerm()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "  Melancholy  " };

            _wordRepositoryMock.Setup(r => r.WordExistsForUserAsync(userId, "melancholy")).ReturnsAsync(false);
            _wordRepositoryMock
                .Setup(r => r.CreatePersonalWordAsync(It.IsAny<Word>(), It.IsAny<UserWord>()))
                .ReturnsAsync((Word w, UserWord uw) => w);

            // Act
            var result = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _wordRepositoryMock.Verify(
                r => r.CreatePersonalWordAsync(
                It.Is<Word>(w => w.Term == "Melancholy"), It.IsAny<UserWord>()), Times.Once);
        }

        [Test]
        public async Task CreatePersonalWordAsync_WithOnlyRequiredField_Succeeds()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreatePersonalWordDTO { Term = "Ubiquitous" };

            _wordRepositoryMock.Setup(r => r.WordExistsForUserAsync(userId, "ubiquitous")).ReturnsAsync(false);
            _wordRepositoryMock
                .Setup(r => r.CreatePersonalWordAsync(It.IsAny<Word>(), It.IsAny<UserWord>()))
                .ReturnsAsync((Word w, UserWord uw) => w);

            // Act
            var result = await _wordService.CreatePersonalWordAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
        }

        [Test]
        public async Task SaveSystemWordAsync_WhenWordIsGlobalAndNotSaved_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-1";
            var systemWord = new Word { Id = 1, Term = "GlobalWord", IsGlobal = true };

            _wordRepositoryMock.Setup(r => r.GetWordAsync(systemWord.Id)).ReturnsAsync(systemWord);
            _wordRepositoryMock.Setup(r => r.UserHasWordAsync(userId, systemWord.Id)).ReturnsAsync(false);

            // Act
            var result = await _wordService.SaveSystemWordAsync(userId, systemWord.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            _wordRepositoryMock.Verify(r => r.SaveUserWordAsync(It.Is<UserWord>(uw => uw.UserId == userId && uw.WordId == systemWord.Id)), Times.Once);
        }

        [Test]
        public async Task SaveSystemWordAsync_WhenWordAlreadySaved_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var systemWord = new Word { Id = 1, Term = "GlobalWord", IsGlobal = true };

            _wordRepositoryMock.Setup(r => r.GetWordAsync(systemWord.Id)).ReturnsAsync(systemWord);
            _wordRepositoryMock.Setup(r => r.UserHasWordAsync(userId, systemWord.Id)).ReturnsAsync(true);

            // Act
            var result = await _wordService.SaveSystemWordAsync(userId, systemWord.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You have already saved this word."));
            _wordRepositoryMock.Verify(r => r.SaveUserWordAsync(It.IsAny<UserWord>()), Times.Never);
        }

        [Test]
        public async Task SaveSystemWordAsync_WhenWordIsNotGlobal_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var personalWord = new Word { Id = 1, Term = "PrivateWord", IsGlobal = false };

            _wordRepositoryMock.Setup(r => r.GetWordAsync(personalWord.Id)).ReturnsAsync(personalWord);

            // Act
            var result = await _wordService.SaveSystemWordAsync(userId, personalWord.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Word not found or is not a system word."));
        }

        [Test]
        public async Task SaveWordToLibraryAsync_WhenWordExistsAndNotSaved_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-1";
            var word = new Word { Id = 4, Term = "Practice" };

            _wordRepositoryMock.Setup(r => r.GetWordAsync(word.Id)).ReturnsAsync(word);
            _wordRepositoryMock.Setup(r => r.UserHasWordAsync(userId, word.Id)).ReturnsAsync(false);

            // Act
            var result = await _wordService.SaveWordToLibraryAsync(userId, word.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _wordRepositoryMock.Verify(
                r => r.SaveUserWordAsync(It.Is<UserWord>(uw => uw.UserId == userId && uw.WordId == word.Id)),
                Times.Once);
        }

        [Test]
        public async Task SaveWordToLibraryAsync_WhenWordAlreadySaved_ReturnsFailure()
        {
            // Arrange
            var userId = "user-1";
            var word = new Word { Id = 7, Term = "Repeat" };

            _wordRepositoryMock.Setup(r => r.GetWordAsync(word.Id)).ReturnsAsync(word);
            _wordRepositoryMock.Setup(r => r.UserHasWordAsync(userId, word.Id)).ReturnsAsync(true);

            // Act
            var result = await _wordService.SaveWordToLibraryAsync(userId, word.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("You have already saved this word."));
            _wordRepositoryMock.Verify(r => r.SaveUserWordAsync(It.IsAny<UserWord>()), Times.Never);
        }

        [Test]
        public async Task GetPersonalWordForEditAsync_UserIsOwner_ReturnsWord()
        {
            // Arrange
            var userId = "user-1";
            var word = new Word { Id = 1, Term = "Existing", IsGlobal = false, CreatorId = userId };
            var userWord = new UserWord { UserId = userId, WordId = word.Id, Word = word };

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(userId, word.Id)).ReturnsAsync(userWord);

            // Act
            var result = await _wordService.GetPersonalWordForEditAsync(userId, word.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(word.Id));
            Assert.That(result.Value.Term, Is.EqualTo("Existing"));
        }

        [Test]
        public async Task GetPersonalWordForEditAsync_UserIsNotOwner_ReturnsFailure()
        {
            // Arrange
            var ownerId = "owner";
            var strangerId = "stranger";
            var word = new Word { Id = 1, Term = "Secret", IsGlobal = false, CreatorId = ownerId };

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(strangerId, word.Id)).ReturnsAsync((UserWord?)null);

            // Act
            var result = await _wordService.GetPersonalWordForEditAsync(strangerId, word.Id);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public async Task UpdateUserWordAsync_OwnerUpdatesDetails_PersistsChanges()
        {
            // Arrange
            var userId = "user-1";
            var word = new Word { Id = 1, Term = "OldTerm", Translation = "OldTrans", IsGlobal = false, CreatorId = userId };
            var userWord = new UserWord { UserId = userId, WordId = word.Id, Word = word };

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(userId, word.Id)).ReturnsAsync(userWord);

            var dto = new UpdateWordDTO
            {
                WordId = word.Id,
                Term = "NewTerm",
                Translation = "NewTrans",
                Meaning = "New Meaning"
            };

            // Act
            var result = await _wordService.UpdateUserWordAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _wordRepositoryMock.Verify(
                r => r.UpdateWordAsync(It.Is<Word>(w =>
                w.Term == "NewTerm" && w.Translation == "NewTrans" && w.Meaning == "New Meaning")), Times.Once);
        }

        [Test]
        public async Task UpdateUserWordAsync_NonOwnerTriesToUpdate_ReturnsFailureAndDoesNotChangeDb()
        {
            // Arrange
            var hackerId = "hacker";
            var wordId = 1;

            _wordRepositoryMock.Setup(r => r.GetUserWordAsync(hackerId, wordId)).ReturnsAsync((UserWord?)null);

            var dto = new UpdateWordDTO { WordId = wordId, Term = "Hacked" };

            // Act
            var result = await _wordService.UpdateUserWordAsync(hackerId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
            _wordRepositoryMock.Verify(r => r.UpdateWordAsync(It.IsAny<Word>()), Times.Never);
        }

        [Test]
        public async Task CreateSystemWordAsync_ValidDTO_CreatesGlobalWord()
        {
            var dto = new CreateSystemWordDTO { Term = "AdminWord", CategoryIds = new List<int>() };
            var result = await _wordService.CreateSystemWordAsync(dto);

            Assert.That(result.IsSuccess, Is.True);
            _wordRepositoryMock.Verify(r => r.CreateWordAsync(It.Is<Word>(w => w.Term == "AdminWord" && w.IsGlobal)), Times.Once);
        }

        [Test]
        public async Task UpdateSystemWordAsync_ValidId_UpdatesGlobalWord()
        {
            var word = new Word { Id = 1, Term = "Old", IsGlobal = true };
            _wordRepositoryMock.Setup(r => r.GetWordAsync(1)).ReturnsAsync(word);

            var dto = new UpdateSystemWordDTO { Id = 1, Term = "New" };
            var result = await _wordService.UpdateSystemWordAsync(1, dto);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(word.Term, Is.EqualTo("New"));
            _wordRepositoryMock.Verify(r => r.UpdateWordAsync(word), Times.Once);
        }

        [Test]
        public async Task DeleteSystemWordAsync_ValidId_DeletesGlobalWord()
        {
            var word = new Word { Id = 1, Term = "AdminWord", IsGlobal = true };
            _wordRepositoryMock.Setup(r => r.GetWordAsync(1)).ReturnsAsync(word);

            var result = await _wordService.DeleteSystemWordAsync(1);

            Assert.That(result.IsSuccess, Is.True);
            _wordRepositoryMock.Verify(r => r.DeleteWordAsync(word), Times.Once);
        }
    }
}
