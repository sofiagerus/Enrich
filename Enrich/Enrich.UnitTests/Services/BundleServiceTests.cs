using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class BundleServiceTests
    {
        private Mock<IBundleRepository> _bundleRepositoryMock = null!;
        private Mock<ILogger<BundleService>> _loggerMock = null!;
        private BundleService _bundleService = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleRepositoryMock = new Mock<IBundleRepository>();
            _loggerMock = new Mock<ILogger<BundleService>>();

            _bundleService = new BundleService(_bundleRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task CreateBundleAsync_WithValidData_CreatesBundleSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreateBundleDTO
            {
                Title = "My English Bundle",
                Description = "A bundle for learning English",
                DifficultyLevels = ["Beginner", "Intermediate"],
                ImageUrl = "https://example.com/image.jpg"
            };

            var createdBundle = new Bundle
            {
                Id = 1,
                Title = dto.Title,
                Description = dto.Description,
                DifficultyLevels = dto.DifficultyLevels,
                ImageUrl = dto.ImageUrl,
                OwnerId = userId,
                Status = BundleStatus.Draft,
                IsSystem = false,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Words = new List<Word>(),
                Categories = new List<Category>(),
                Tags = new List<Tag>()
            };

            _bundleRepositoryMock
                .Setup(r => r.BundleTitleExistsForUserAsync(userId, "my english bundle"))
                .ReturnsAsync(false);

            _bundleRepositoryMock
                .Setup(r => r.CreateBundleAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(createdBundle);

            // Act
            var result = await _bundleService.CreateBundleAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ErrorMessage, Is.Null);
            _bundleRepositoryMock.Verify(r => r.CreateBundleAsync(It.IsAny<Bundle>()), Times.Once);
        }

        [Test]
        public async Task CreateBundleAsync_WithEmptyTitle_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreateBundleDTO
            {
                Title = "  ",
                Description = "A bundle for learning English"
            };

            // Act
            var result = await _bundleService.CreateBundleAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("не може бути порожною"));
            _bundleRepositoryMock.Verify(r => r.CreateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task CreateBundleAsync_WithDuplicateTitle_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var dto = new CreateBundleDTO
            {
                Title = "My English Bundle",
                Description = "A bundle for learning English"
            };

            _bundleRepositoryMock
                .Setup(r => r.BundleTitleExistsForUserAsync(userId, "my english bundle"))
                .ReturnsAsync(true);

            // Act
            var result = await _bundleService.CreateBundleAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("вже маєте бандл"));
            _bundleRepositoryMock.Verify(r => r.CreateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task CreateBundleAsync_WithWords_AddsWordsSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var wordIds = new[] { 1, 2, 3 };
            var dto = new CreateBundleDTO
            {
                Title = "My English Bundle",
                WordIds = wordIds
            };

            var createdBundle = new Bundle
            {
                Id = 1,
                Title = dto.Title,
                OwnerId = userId,
                Status = BundleStatus.Draft,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Words = new List<Word>(),
                Categories = new List<Category>(),
                Tags = new List<Tag>()
            };

            _bundleRepositoryMock
                .Setup(r => r.BundleTitleExistsForUserAsync(userId, "my english bundle"))
                .ReturnsAsync(false);

            _bundleRepositoryMock
                .Setup(r => r.CreateBundleAsync(It.IsAny<Bundle>()))
                .ReturnsAsync(createdBundle);

            _bundleRepositoryMock
                .Setup(r => r.AddWordsToBundleAsync(1, wordIds))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bundleService.CreateBundleAsync(userId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(r => r.AddWordsToBundleAsync(1, wordIds), Times.Once);
        }

        [Test]
        public async Task GetBundleByIdAsync_WithValidId_ReturnsBundleDTO()
        {
            // Arrange
            var bundleId = 1;
            var bundle = new Bundle
            {
                Id = bundleId,
                Title = "Test Bundle",
                Description = "Test Description",
                OwnerId = "user-1",
                Status = BundleStatus.Draft,
                IsPublic = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Words = new List<Word> { new Word { Id = 1, Term = "Apple" } },
                Categories = new List<Category> { new Category { Id = 1, Name = "Fruits" } },
                Tags = new List<Tag>()
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdWithDetailsAsync(bundleId))
                .ReturnsAsync(bundle);

            // Act
            var result = await _bundleService.GetBundleByIdAsync(bundleId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(bundleId));
            Assert.That(result.Title, Is.EqualTo("Test Bundle"));
            Assert.That(result.WordCount, Is.EqualTo(1));
            Assert.That(result.CategoryCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetBundleByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var bundleId = 999;

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdWithDetailsAsync(bundleId))
                .ReturnsAsync((Bundle?)null);

            // Act
            var result = await _bundleService.GetBundleByIdAsync(bundleId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUserBundlesAsync_ReturnsAllUserBundles()
        {
            // Arrange
            var userId = "user-1";
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Bundle 1",
                    OwnerId = userId,
                    Status = BundleStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Words = new List<Word>(),
                    Categories = new List<Category>(),
                    Tags = new List<Tag>()
                },
                new Bundle
                {
                    Id = 2,
                    Title = "Bundle 2",
                    OwnerId = userId,
                    Status = BundleStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Words = new List<Word>(),
                    Categories = new List<Category>(),
                    Tags = new List<Tag>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetUserBundlesAsync(userId))
                .ReturnsAsync(bundles);

            // Act
            var result = await _bundleService.GetUserBundlesAsync(userId);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            _bundleRepositoryMock.Verify(r => r.GetUserBundlesAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetUserBundlesPageAsync_WithFilters_ReturnsPagedResult()
        {
            // Arrange
            var userId = "user-1";
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "English Bundle",
                    OwnerId = userId,
                    Status = BundleStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Words = new List<Word>(),
                    Categories = new List<Category>(),
                    Tags = new List<Tag>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetUserBundlesPageAsync(userId, "English", 1, 10))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetUserBundlesPageAsync(userId, "English", 1, 10);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            _bundleRepositoryMock.Verify(r => r.GetUserBundlesPageAsync(userId, "English", 1, 10), Times.Once);
        }

        [Test]
        public async Task UpdateBundleAsync_ByOwner_UpdatesSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var bundleId = 1;
            var dto = new CreateBundleDTO
            {
                Title = "Updated Bundle",
                Description = "Updated description"
            };

            var existingBundle = new Bundle
            {
                Id = bundleId,
                Title = "Old Bundle",
                OwnerId = userId,
                Status = BundleStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            _bundleRepositoryMock
                .Setup(r => r.BundleTitleExistsForUserAsync(userId, "updated bundle"))
                .ReturnsAsync(false);

            _bundleRepositoryMock
                .Setup(r => r.UpdateBundleAsync(It.IsAny<Bundle>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bundleService.UpdateBundleAsync(userId, bundleId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(It.IsAny<Bundle>()), Times.Once);
        }

        [Test]
        public async Task UpdateBundleAsync_NotOwner_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var otherUserId = "user-2";
            var bundleId = 1;
            var dto = new CreateBundleDTO { Title = "Updated Bundle" };

            var existingBundle = new Bundle
            {
                Id = bundleId,
                Title = "Old Bundle",
                OwnerId = otherUserId,
                Status = BundleStatus.Draft
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            // Act
            var result = await _bundleService.UpdateBundleAsync(userId, bundleId, dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("не маєте права"));
            _bundleRepositoryMock.Verify(r => r.UpdateBundleAsync(It.IsAny<Bundle>()), Times.Never);
        }

        [Test]
        public async Task DeleteBundleAsync_ByOwner_DeletesSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var bundleId = 1;

            var existingBundle = new Bundle
            {
                Id = bundleId,
                Title = "Bundle to Delete",
                OwnerId = userId,
                Status = BundleStatus.Draft
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            _bundleRepositoryMock
                .Setup(r => r.DeleteBundleAsync(bundleId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bundleService.DeleteBundleAsync(userId, bundleId);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(r => r.DeleteBundleAsync(bundleId), Times.Once);
        }

        [Test]
        public async Task DeleteBundleAsync_NotOwner_ReturnsError()
        {
            // Arrange
            var userId = "user-1";
            var otherUserId = "user-2";
            var bundleId = 1;

            var existingBundle = new Bundle
            {
                Id = bundleId,
                Title = "Bundle to Delete",
                OwnerId = otherUserId,
                Status = BundleStatus.Draft
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            // Act
            var result = await _bundleService.DeleteBundleAsync(userId, bundleId);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("не маєте права"));
            _bundleRepositoryMock.Verify(r => r.DeleteBundleAsync(bundleId), Times.Never);
        }

        [Test]
        public async Task AddWordsToBundleAsync_ByOwner_AddsWordsSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var bundleId = 1;
            var wordIds = new[] { 1, 2, 3 };

            var existingBundle = new Bundle
            {
                Id = bundleId,
                OwnerId = userId,
                Title = "Test Bundle",
                Status = BundleStatus.Draft
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            _bundleRepositoryMock
                .Setup(r => r.AddWordsToBundleAsync(bundleId, wordIds))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bundleService.AddWordsToBundleAsync(userId, bundleId, wordIds);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(r => r.AddWordsToBundleAsync(bundleId, wordIds), Times.Once);
        }

        [Test]
        public async Task RemoveWordsFromBundleAsync_ByOwner_RemovesWordsSuccessfully()
        {
            // Arrange
            var userId = "user-1";
            var bundleId = 1;
            var wordIds = new[] { 1, 2 };

            var existingBundle = new Bundle
            {
                Id = bundleId,
                OwnerId = userId,
                Title = "Test Bundle",
                Status = BundleStatus.Draft
            };

            _bundleRepositoryMock
                .Setup(r => r.GetBundleByIdAsync(bundleId))
                .ReturnsAsync(existingBundle);

            _bundleRepositoryMock
                .Setup(r => r.RemoveWordsFromBundleAsync(bundleId, wordIds))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bundleService.RemoveWordsFromBundleAsync(userId, bundleId, wordIds);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            _bundleRepositoryMock.Verify(r => r.RemoveWordsFromBundleAsync(bundleId, wordIds), Times.Once);
        }
    }
}
