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
        private Mock<ICategoryRepository> _categoryRepositoryMock = null!;
        private Mock<ILogger<BundleService>> _loggerMock = null!;
        private Mock<IOptions<PaginationSettings>> _paginationOptionsMock = null!;
        private BundleService _bundleService = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleRepositoryMock = new Mock<IBundleRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<BundleService>>();

            _paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();
            _paginationOptionsMock.Setup(o => o.Value).Returns(new PaginationSettings());

            _bundleService = new BundleService(
                _bundleRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _paginationOptionsMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task GetSystemBundlesAsync_ReturnsPagedResult_WithCorrectMapping()
        {
            // Arrange
            // ВИПРАВЛЕНО CS0029: Судячи з помилки, твоя сутність очікує string[], а не List<string>
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
                        // ВИПРАВЛЕНО CS0117: Властивість називається Meaning
                        new Word { Id = 1, Term = "Word1", Meaning = "Def1" },
                        new Word { Id = 2, Term = "Word2", Meaning = "Def2" }
                    },
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Fruits" }
                    }
                }
            };

            // ВИПРАВЛЕНО SA1116: Параметри мають починатися з нового рядка, якщо їх багато
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
            // ВИПРАВЛЕНО: Використовуємо CreateBundleDTO
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
    }
}