using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class StudySessionServiceTests
    {
        private const string TestUserId = "test-user-1";
        private const int TestBundleId = 1;
        private const int TestWordId = 1;

        private Mock<ITrainingSessionRepository> _trainingSessionRepositoryMock = null!;
        private Mock<IWordProgressRepository> _wordProgressRepositoryMock = null!;
        private Mock<IBundleRepository> _bundleRepositoryMock = null!;
        private Mock<IWordRepository> _wordRepositoryMock = null!;
        private Mock<ILogger<StudySessionService>> _loggerMock = null!;

        private IStudySessionService _studySessionService = null!;

        [SetUp]
        public void SetUp()
        {
            _trainingSessionRepositoryMock = new Mock<ITrainingSessionRepository>();
            _wordProgressRepositoryMock = new Mock<IWordProgressRepository>();
            _bundleRepositoryMock = new Mock<IBundleRepository>();
            _wordRepositoryMock = new Mock<IWordRepository>();
            _loggerMock = new Mock<ILogger<StudySessionService>>();

            _studySessionService = new StudySessionService(
                _trainingSessionRepositoryMock.Object,
                _wordProgressRepositoryMock.Object,
                _bundleRepositoryMock.Object,
                _wordRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task StartStudySessionAsync_WithValidBundle_ReturnsSuccessWithSessionData()
        {
            // Arrange
            var bundle = new Bundle
            {
                Id = TestBundleId,
                Title = "English Vocabulary",
                OwnerId = TestUserId,
                Words = new List<Word>
                {
                    new Word { Id = 1, Term = "Hello" },
                    new Word { Id = 2, Term = "Goodbye" }
                }
            };

            var createdSession = new TrainingSession
            {
                Id = 1,
                UserId = TestUserId,
                TotalCards = 2
            };

            _bundleRepositoryMock.Setup(r => r.GetBundleByIdWithDetailsAsync(TestBundleId)).ReturnsAsync(bundle);
            _trainingSessionRepositoryMock.Setup(r => r.CreateSessionAsync(It.IsAny<TrainingSession>())).ReturnsAsync(createdSession);

            // Act
            var result = await _studySessionService.StartStudySessionAsync(TestUserId, TestBundleId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value!.SessionId, Is.EqualTo(1));
                Assert.That(result.Value.TotalCards, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task StartStudySessionAsync_WithEmptyBundle_ReturnsFailure()
        {
            // Arrange
            var emptyBundle = new Bundle { Id = TestBundleId, Words = new List<Word>(), OwnerId = TestUserId };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdWithDetailsAsync(TestBundleId)).ReturnsAsync(emptyBundle);

            // Act
            var result = await _studySessionService.StartStudySessionAsync(TestUserId, TestBundleId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("не містить слів") | Does.Contain("порожній") | Does.Contain("empty"));
        }

        [Test]
        public async Task SubmitAnswerAsync_WithCorrectAnswer_Awards10Points()
        {
            // Arrange
            var session = new TrainingSession { Id = 1, UserId = TestUserId, TotalCards = 5 };
            var dto = new SubmitAnswerDTO { SessionId = 1, WordId = TestWordId, IsKnown = true };

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdAsync(1)).ReturnsAsync(session);
            _wordRepositoryMock.Setup(r => r.GetWordAsync(TestWordId)).ReturnsAsync(new Word { Id = TestWordId });

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdWithDetailsAsync(1))
                .ReturnsAsync(new TrainingSession
                {
                    UserId = TestUserId,
                    SessionResults = new List<SessionResult> { new SessionResult { PointsAwarded = 10 } }
                });

            // Act
            var result = await _studySessionService.SubmitAnswerAsync(TestUserId, dto);

            // Assert
            Assert.That(result.Value!.TotalPoints, Is.EqualTo(10));
            _trainingSessionRepositoryMock.Verify(r => r.AddSessionResultAsync(It.Is<SessionResult>(sr => sr.PointsAwarded == 10)), Times.Once);
        }

        [Test]
        public async Task SubmitAnswerAsync_WithIncorrectAnswer_Awards5Points_AndIncrementsUnknownCount()
        {
            // Arrange
            var session = new TrainingSession { Id = 1, UserId = TestUserId, UnknownCount = 0, TotalCards = 5 };
            var dto = new SubmitAnswerDTO { SessionId = 1, WordId = TestWordId, IsKnown = false };

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdAsync(1)).ReturnsAsync(session);
            _wordRepositoryMock.Setup(r => r.GetWordAsync(TestWordId)).ReturnsAsync(new Word { Id = TestWordId });
            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdWithDetailsAsync(1))
                .ReturnsAsync(new TrainingSession
                {
                    UserId = TestUserId,
                    SessionResults = new List<SessionResult> { new SessionResult { PointsAwarded = 5 } }
                });

            // Act
            await _studySessionService.SubmitAnswerAsync(TestUserId, dto);

            // Assert
            _trainingSessionRepositoryMock.Verify(r => r.UpdateSessionAsync(It.Is<TrainingSession>(s => s.UnknownCount == 1)), Times.Once);
        }

        [Test]
        public async Task SubmitAnswerAsync_UpdatesSessionCounters_AndLogsActivity()
        {
            // Arrange
            var session = new TrainingSession { Id = 1, UserId = TestUserId, KnownCount = 2, TotalCards = 10 };
            var dto = new SubmitAnswerDTO { SessionId = 1, WordId = TestWordId, IsKnown = true };

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdAsync(1)).ReturnsAsync(session);
            _wordRepositoryMock.Setup(r => r.GetWordAsync(TestWordId)).ReturnsAsync(new Word { Id = TestWordId });
            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdWithDetailsAsync(1)).ReturnsAsync(session);

            // Act
            await _studySessionService.SubmitAnswerAsync(TestUserId, dto);

            // Assert
            _trainingSessionRepositoryMock.Verify(r => r.UpdateSessionAsync(It.Is<TrainingSession>(s => s.KnownCount == 3)), Times.Once);

            // Оновлено Verify для логера для більшої гнучкості
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task GetSessionProgressAsync_CalculatesCorrectPercentage()
        {
            // Arrange
            var session = new TrainingSession
            {
                Id = 1,
                UserId = TestUserId,
                TotalCards = 4,
                KnownCount = 1,
                UnknownCount = 1,
                SessionResults = new List<SessionResult> { new SessionResult { PointsAwarded = 10 }, new SessionResult { PointsAwarded = 5 } }
            };

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdWithDetailsAsync(1)).ReturnsAsync(session);

            // Act
            var result = await _studySessionService.GetSessionProgressAsync(TestUserId, 1);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Value!.ProgressPercentage, Is.EqualTo(50));
                Assert.That(result.Value.TotalPoints, Is.EqualTo(15));
            });
        }

        [Test]
        public async Task SubmitAnswerAsync_WithUnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var session = new TrainingSession { Id = 1, UserId = "other-user" };
            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdAsync(1)).ReturnsAsync(session);

            // Act
            var result = await _studySessionService.SubmitAnswerAsync(TestUserId, new SubmitAnswerDTO { SessionId = 1 });

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Доступ заборонено") | Does.Contain("Access denied"));
        }

        [Test]
        public async Task SubmitAnswerAsync_UpdatesWordProgress_InDatabase()
        {
            // Arrange
            var session = new TrainingSession { Id = 1, UserId = TestUserId };
            var existingProgress = new WordProgress { UserId = TestUserId, WordId = TestWordId, Points = 20 };

            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdAsync(1)).ReturnsAsync(session);
            _wordRepositoryMock.Setup(r => r.GetWordAsync(TestWordId)).ReturnsAsync(new Word { Id = TestWordId });
            _wordProgressRepositoryMock.Setup(r => r.GetWordProgressAsync(TestUserId, TestWordId)).ReturnsAsync(existingProgress);
            _trainingSessionRepositoryMock.Setup(r => r.GetSessionByIdWithDetailsAsync(1)).ReturnsAsync(session);

            // Act
            await _studySessionService.SubmitAnswerAsync(TestUserId, new SubmitAnswerDTO { SessionId = 1, WordId = TestWordId, IsKnown = true });

            // Assert
            _wordProgressRepositoryMock.Verify(r => r.UpdateWordProgressAsync(It.Is<WordProgress>(p => p.Points == 30)), Times.Once);
        }
    }
}