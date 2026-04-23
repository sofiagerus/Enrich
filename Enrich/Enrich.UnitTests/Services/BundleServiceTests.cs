using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class BundleServiceTests
    {
        private const string TestUserId = "user-123";

        private Mock<IBundleRepository> _bundleRepositoryMock = null!;
        private Mock<IWordRepository> _wordRepositoryMock = null!;
        private Mock<ICategoryRepository> _categoryRepositoryMock = null!;
        private Mock<ILogger<BundleService>> _loggerMock = null!;
        private Mock<IOptions<PaginationSettings>> _paginationOptionsMock = null!;
        private BundleService _bundleService = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleRepositoryMock = new Mock<IBundleRepository>();
            _wordRepositoryMock = new Mock<IWordRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<BundleService>>();

            _paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();
            _paginationOptionsMock.Setup(o => o.Value).Returns(new PaginationSettings());

            _bundleService = new BundleService(
                _bundleRepositoryMock.Object,
                _wordRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _paginationOptionsMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task GetBundleByIdAsync_ValidId_ReturnsBundleWithWords()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle
            {
                Id = bundleId,
                Title = "Test Bundle",
                Words = new List<Word> { new Word { Id = 1, Term = "Apple", Translation = "Яблуко" } }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdWithDetailsAsync(bundleId))
                .ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.GetBundleByIdAsync(bundleId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Words.Count, Is.EqualTo(1));
            Assert.That(result.Words[0].Term, Is.EqualTo("Apple"));
        }

        [Test]
        public async Task GetSystemBundlesAsync_ReturnsPagedResult()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Test Bundle",
                    Description = "Test Description",
                    DifficultyLevels = new[] { "A1", "A2" },
                    IsSystem = true,
                    Words = new List<Word>
                    {
                        new Word { Id = 1, Term = "Word1", Meaning = "Def1" },
                        new Word { Id = 2, Term = "Word2", Meaning = "Def2" }
                    },
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Fruits" }
                    }
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalCount, Is.EqualTo(1));
                Assert.That(result.Items.Count(), Is.EqualTo(1));
                var bundle = result.Items.First();
                Assert.That(bundle.Title, Is.EqualTo("Test Bundle"));
                Assert.That(bundle.WordCount, Is.EqualTo(2));
                Assert.That(bundle.Categories, Contains.Item("Fruits"));
            });
        }

        [Test]
        public async Task SaveSystemBundleAsync_ValidBundle_CreatesUserLink()
        {
            // Arrange
            var bundleId = 1;
            var systemBundle = new Bundle { Id = bundleId, IsSystem = true };

            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(systemBundle);
            _bundleRepositoryMock.Setup(r => r.UserHasBundleAsync(TestUserId, bundleId)).ReturnsAsync(false);

            // Act
            var result = await _bundleService.SaveSystemBundleAsync(TestUserId, bundleId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(
                r => r.SaveUserBundleAsync(It.Is<UserBundle>(ub =>
                ub.UserId == TestUserId && ub.BundleId == bundleId)), Times.Once);
        }

        [Test]
        public async Task UpdateBundleAsync_WhenPublished_ReturnsError()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle
            {
                Id = bundleId,
                OwnerId = TestUserId,
                Status = BundleStatus.Published
            };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.UpdateBundleAsync(TestUserId, bundleId, new CreateBundleDTO { Title = "New" });

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("cannot edit"));
        }

        [Test]
        public async Task SubmitBundleForReviewAsync_FromDraft_UpdatesStatus()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle
            {
                Id = bundleId,
                OwnerId = TestUserId,
                Status = BundleStatus.Draft
            };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.SubmitBundleForReviewAsync(TestUserId, bundleId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(bundle.Status, Is.EqualTo(BundleStatus.PendingReview));
            });
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(bundle), Times.Once);
        }

        [Test]
        public async Task DeleteBundleAsync_WithWrongOwner_ReturnsError()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle { Id = bundleId, OwnerId = "other-user" };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.DeleteBundleAsync(TestUserId, bundleId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            _bundleRepositoryMock.Verify(
                r => r.DeleteBundleAsync(It.Is<int>(id => id == bundleId)), Times.Never);
        }

        [Test]
        public async Task GetBundleWithWordsAsync_ExistingBundle_ReturnsMappedDTO()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle
            {
                Id = bundleId,
                Title = "Test Bundle",
                Words = new List<Word>
                {
                    new Word { Id = 1, Term = "Apple", Translation = "Яблуко", PartOfSpeech = "noun", Example = "An apple a day." }
                },
                Categories = new List<Category> { new Category { Id = 2, Name = "Fruits" } },
                Tags = new List<Tag>()
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdWithDetailsAsync(bundleId))
                .ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.GetBundleWithWordsAsync(bundleId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bundleId));
            Assert.That(result.Words.Count, Is.EqualTo(1));
            Assert.That(result.Words[0].Term, Is.EqualTo("Apple"));
            Assert.That(result.Words[0].PartOfSpeech, Is.EqualTo("noun"));
            Assert.That(result.Categories.Count, Is.EqualTo(1));
            Assert.That(result.Categories[0], Is.EqualTo("Fruits"));
            _bundleRepositoryMock.Verify(r => r.GetBundleByIdWithDetailsAsync(bundleId), Times.Once);
        }

        [Test]
        public async Task GetBundleWithWordsAsync_NonExistentBundle_ReturnsNull()
        {
            // Arrange
            var bundleId = 999;

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdWithDetailsAsync(bundleId))
                .ReturnsAsync((Bundle?)null);

            // Act
            var result = await _bundleService.GetBundleWithWordsAsync(bundleId);

            // Assert
            Assert.That(result, Is.Null);
            _bundleRepositoryMock.Verify(r => r.GetBundleByIdWithDetailsAsync(bundleId), Times.Once);
        }

        [Test]
        public async Task GetCommunityBundlesAsync_ReturnsPagedResult()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Community Bundle 1",
                    Description = "Test Description",
                    DifficultyLevels = ["B1", "B2"],
                    Words = new List<Word> { new Word { Id = 1, Term = "Test" } },
                    Categories = new List<Category> { new Category { Id = 1, Name = "Education" } }
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetCommunityBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetCommunityBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Page, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(12));

            var bundle = result.Items.First();
            Assert.That(bundle.Id, Is.EqualTo(1));
            Assert.That(bundle.Title, Is.EqualTo("Community Bundle 1"));
            Assert.That(bundle.WordCount, Is.EqualTo(1));
            Assert.That(bundle.Categories.Count, Is.EqualTo(1));

            _bundleRepositoryMock.Verify(
                r => r.GetCommunityBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetCommunityBundlesAsync_WithInvalidPagination_DefaultsToValidValues()
        {
            // Arrange
            _bundleRepositoryMock
                .Setup(r => r.GetCommunityBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((new List<Bundle>().AsEnumerable(), 0));

            // Act
            // Передаємо page = 0 (невалідно) та pageSize = 0 (невалідно)
            var result = await _bundleService.GetCommunityBundlesAsync(null, null, null, null, null, 0, 0);

            // Assert
            Assert.That(result.Page, Is.EqualTo(1)); // Має скинутися на 1
            Assert.That(result.PageSize, Is.EqualTo(12)); // Має скинутися на дефолтні 12

            _bundleRepositoryMock.Verify(
                r => r.GetCommunityBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetPendingBundlesAsync_ReturnsPagedResult()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Pending Bundle 1",
                    Description = "Test Description",
                    DifficultyLevels = ["B1", "B2"],
                    Words = new List<Word> { new Word { Id = 1, Term = "Test" } },
                    Categories = new List<Category> { new Category { Id = 1, Name = "Education" } },
                    Status = BundleStatus.PendingReview
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetPendingBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetPendingBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Page, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(12));

            var bundle = result.Items.First();
            Assert.That(bundle.Id, Is.EqualTo(1));
            Assert.That(bundle.Title, Is.EqualTo("Pending Bundle 1"));
            Assert.That(bundle.WordCount, Is.EqualTo(1));
            Assert.That(bundle.Categories.Count, Is.EqualTo(1));

            _bundleRepositoryMock.Verify(
                r => r.GetPendingBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task ReviewBundleAsync_BundleNotFound_ReturnsError()
        {
            // Arrange
            var bundleId = 1;
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync((Bundle?)null);

            // Act
            var result = await _bundleService.ReviewBundleAsync(bundleId, true);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Bundle not found."));
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task ReviewBundleAsync_BundleNotPending_ReturnsError()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle { Id = bundleId, Status = BundleStatus.Draft };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.ReviewBundleAsync(bundleId, true);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Bundle is not pending review."));
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task ReviewBundleAsync_ApproveTrue_SetsPublishedStatus()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle { Id = bundleId, Status = BundleStatus.PendingReview };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.ReviewBundleAsync(bundleId, true);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(bundle.Status, Is.EqualTo(BundleStatus.Published));
            Assert.That(bundle.ReviewedAt, Is.Not.Null);
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(bundle), Times.Once);
        }

        [Test]
        public async Task ReviewBundleAsync_ApproveFalse_SetsRejectedStatus()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle { Id = bundleId, Status = BundleStatus.PendingReview };
            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId)).ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.ReviewBundleAsync(bundleId, false);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(bundle.Status, Is.EqualTo(BundleStatus.Rejected));
            Assert.That(bundle.ReviewedAt, Is.Not.Null);
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(bundle), Times.Once);
        }

        [Test]
        public async Task GenerateBundleAsync_WithValidRules_ReturnsGeneratedWords()
        {
            // Arrange
            var dto = new GenerateBundleDTO
            {
                Title = "Custom Generated",
                Rules = new List<BundleGenerationRuleDTO>
                {
                    new BundleGenerationRuleDTO { CategoryId = 1, WordCount = 2 }
                }
            };

            var words = new List<Word>
            {
                new Word { Id = 1, Term = "Word1", DifficultyLevel = "A1", Categories = new List<Category> { new Category { Name = "General" } } },
                new Word { Id = 2, Term = "Word2", DifficultyLevel = "A2", Categories = new List<Category> { new Category { Name = "General" } } }
            };

            _wordRepositoryMock
                .Setup(r => r.GetRandomWordsByCriteriaAsync(1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 2))
                .ReturnsAsync(words);

            // Act
            var result = await _bundleService.GenerateBundleAsync(TestUserId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Title, Is.EqualTo("Custom Generated"));
            Assert.That(result.Value.Words.Count, Is.EqualTo(2));
            Assert.That(result.Value.Words[0].Term, Is.EqualTo("Word1"));
            _bundleRepositoryMock.Verify(r => r.CreateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task GenerateBundleAsync_NoWordsFound_ReturnsError()
        {
            // Arrange
            var dto = new GenerateBundleDTO
            {
                Title = "Empty Bundle",
                Rules = new List<BundleGenerationRuleDTO>
                {
                    new BundleGenerationRuleDTO { CategoryId = 1, WordCount = 5 }
                }
            };

            _wordRepositoryMock
                .Setup(r => r.GetRandomWordsByCriteriaAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Word>());

            // Act
            var result = await _bundleService.GenerateBundleAsync(TestUserId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("No words found"));
            _bundleRepositoryMock.Verify(r => r.CreateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task CreateSystemBundleAsync_ValidDto_SetsSystemFlagsAndStatus()
        {
            // Arrange
            var dto = new CreateBundleDTO
            {
                Title = "New System Collection",
                CategoryIds = new List<int> { 1 },
                WordIds = new List<int> { 10, 11 }
            };

            _bundleRepositoryMock
                .Setup(r => r.CreateBundleAsync(It.IsAny<Bundle>()))
                .ReturnsAsync((Bundle b) =>
                {
                    b.Id = 100;
                    return b;
                });

            // Act
            var result = await _bundleService.CreateSystemBundleAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(
                r => r.CreateBundleAsync(It.Is<Bundle>(b =>
                b.Title == "New System Collection" &&
                b.IsSystem &&
                b.OwnerId == "SYSTEM" &&
                b.Status == BundleStatus.Published)),
                Times.Once);

            _bundleRepositoryMock.Verify(r => r.AddWordsToBundleAsync(100, dto.WordIds), Times.Once);
            _bundleRepositoryMock.Verify(r => r.AddCategoriesToBundleAsync(100, dto.CategoryIds), Times.Once);
        }

        [Test]
        public async Task UpdateSystemBundleAsync_ExistingSystemBundle_UpdatesAndSyncs()
        {
            // Arrange
            var bundleId = 55;
            var systemBundle = new Bundle { Id = bundleId, IsSystem = true, Title = "Old Title" };
            var dto = new CreateBundleDTO
            {
                Title = "Updated Title",
                WordIds = new List<int> { 1, 2 },
                CategoryIds = new List<int> { 3 }
            };

            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(systemBundle);

            // Act
            var result = await _bundleService.UpdateSystemBundleAsync(bundleId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(systemBundle.Title, Is.EqualTo("Updated Title"));

            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(systemBundle), Times.Once);
            _bundleRepositoryMock.Verify(r => r.SyncBundleRelationsAsync(bundleId, dto.WordIds, dto.CategoryIds), Times.Once);
        }

        [Test]
        public async Task UpdateSystemBundleAsync_NonSystemBundle_ReturnsError()
        {
            // Arrange
            var bundleId = 1;
            var userBundle = new Bundle { Id = bundleId, IsSystem = false };
            var dto = new CreateBundleDTO { Title = "Hacker Title" };

            _bundleRepositoryMock.Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(userBundle);

            // Act
            var result = await _bundleService.UpdateSystemBundleAsync(bundleId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("System bundle not found."));
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }
    }
}