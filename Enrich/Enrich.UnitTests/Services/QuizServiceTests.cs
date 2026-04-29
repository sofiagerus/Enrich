using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class QuizServiceTests
    {
        private Mock<IWordRepository> _wordRepositoryMock = null!;
        private Mock<ITrainingSessionRepository> _trainingSessionRepositoryMock = null!;
        private Mock<ILogger<QuizService>> _loggerMock = null!;
        private QuizService _quizService = null!;

        [SetUp]
        public void SetUp()
        {
            _wordRepositoryMock = new Mock<IWordRepository>();
            _trainingSessionRepositoryMock = new Mock<ITrainingSessionRepository>();
            _loggerMock = new Mock<ILogger<QuizService>>();

            _quizService = new QuizService(
                _wordRepositoryMock.Object,
                _trainingSessionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task StartCustomQuizAsync_WhenWordsFound_CreatesSessionAndReturnsDto()
        {
            // Arrange
            var userId = "user-1";
            var setup = new QuizSetupDTO { WordCount = 5 };
            var words = new List<Word>
            {
                new Word { Id = 1, Term = "Word1", Translation = "Trans1" },
                new Word { Id = 2, Term = "Word2", Translation = "Trans2" }
            };

            _wordRepositoryMock
                .Setup(r => r.GetRandomPersonalWordsByCriteriaAsync(userId, null, null, null, null, 5))
                .ReturnsAsync(words);

            _trainingSessionRepositoryMock
                .Setup(r => r.CreateSessionAsync(It.IsAny<TrainingSession>()))
                .ReturnsAsync((TrainingSession s) =>
                {
                    s.Id = 100;
                    return s;
                });

            // Act
            var result = await _quizService.StartCustomQuizAsync(userId, setup);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.SessionId, Is.EqualTo(100));
            Assert.That(result.Value.Cards.Count, Is.EqualTo(2));
            Assert.That(result.Value.Cards[0].Term, Is.EqualTo("Word1"));
            _trainingSessionRepositoryMock.Verify(r => r.CreateSessionAsync(It.Is<TrainingSession>(s => s.UserId == userId && s.BundleId == null)), Times.Once);
        }

        [Test]
        public async Task StartCustomQuizAsync_WhenNoWordsFound_ReturnsFailure()
        {
            // Arrange
            var userId = "user-1";
            var setup = new QuizSetupDTO { WordCount = 5 };

            _wordRepositoryMock
                .Setup(r => r.GetRandomPersonalWordsByCriteriaAsync(userId, null, null, null, null, 5))
                .ReturnsAsync(new List<Word>());

            // Act
            var result = await _quizService.StartCustomQuizAsync(userId, setup);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("No words found matching the specified criteria."));
            _trainingSessionRepositoryMock.Verify(r => r.CreateSessionAsync(It.IsAny<TrainingSession>()), Times.Never);
        }

        [Test]
        public async Task GetAvailableCategoriesAsync_ReturnsDistinctCategories()
        {
            // Arrange
            var userId = "user-1";
            var cat1 = new Category { Name = "Tech" };
            var cat2 = new Category { Name = "Science" };
            var userWords = new List<UserWord>
            {
                new UserWord { UserId = userId, Word = new Word { Categories = new List<Category> { cat1 } } },
                new UserWord { UserId = userId, Word = new Word { Categories = new List<Category> { cat1, cat2 } } }
            };

            _wordRepositoryMock.Setup(r => r.GetPersonalWordsWithDetailsAsync(userId)).ReturnsAsync(userWords);

            // Act
            var result = await _quizService.GetAvailableCategoriesAsync(userId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            var list = result.Value!.ToList();
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list, Contains.Item("Tech"));
            Assert.That(list, Contains.Item("Science"));
        }
    }
}
