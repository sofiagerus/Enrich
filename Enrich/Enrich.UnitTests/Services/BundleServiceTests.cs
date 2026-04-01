using Enrich.BLL.Services;
using Enrich.DAL.Entities;
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
        private Mock<ICategoryRepository> _categoryRepositoryMock = null!;
        private Mock<ILogger<BundleService>> _loggerMock = null!;
        private BundleService _bundleService = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleRepositoryMock = new Mock<IBundleRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<BundleService>>();

            _bundleService = new BundleService(
                _bundleRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _loggerMock.Object);
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
                    DifficultyLevels = ["A1", "A2"],
                    Words = new List<Word>
                    {
                        new Word { Id = 1, Term = "Word1" },
                        new Word { Id = 2, Term = "Word2" }
                    },
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Fruits" }
                    }
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Page, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(12));

            var bundle = result.Items.First();
            Assert.That(bundle.Id, Is.EqualTo(1));
            Assert.That(bundle.Title, Is.EqualTo("Test Bundle"));
            Assert.That(bundle.WordCount, Is.EqualTo(2));
            Assert.That(bundle.Categories.Count, Is.EqualTo(1));
            Assert.That(bundle.Categories[0], Is.EqualTo("Fruits"));

            _bundleRepositoryMock.Verify(
                r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithSearchTerm_FiltersResults()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Fruits Bundle",
                    Description = "Learn fruit vocabulary",
                    DifficultyLevels = ["B1"],
                    Words = new List<Word> { new Word { Id = 1, Term = "Apple" } },
                    Categories = new List<Category>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync("fruit", null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync("fruit", null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().Title, Is.EqualTo("Fruits Bundle"));
            _bundleRepositoryMock.Verify(
                r => r.GetSystemBundlesPageAsync("fruit", null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithCategoryFilter_FiltersResults()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Tech Bundle",
                    DifficultyLevels = [],
                    Words = new List<Word>(),
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Technology" }
                    }
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, "Technology", null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, "Technology", null, null, null, 1, 12);

            // Assert
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().Categories[0], Is.EqualTo("Technology"));
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithDifficultyFilter_FiltersResults()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Beginner Bundle",
                    DifficultyLevels = ["A1", "A2"],
                    Words = new List<Word>(),
                    Categories = new List<Category>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, "A1,A2", null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, "A1,A2", null, null, 1, 12);

            // Assert
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().DifficultyLevels, Contains.Item("A1"));
            Assert.That(result.Items.First().DifficultyLevels, Contains.Item("A2"));
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithWordCountRange_FiltersResults()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Medium Bundle",
                    DifficultyLevels = [],
                    Words = new List<Word>
                    {
                        new Word { Id = 1, Term = "Word1" },
                        new Word { Id = 2, Term = "Word2" },
                        new Word { Id = 3, Term = "Word3" }
                    },
                    Categories = new List<Category>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, 2, 5, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, 2, 5, 1, 12);

            // Assert
            Assert.That(result.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Items.First().WordCount, Is.EqualTo(3));
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithInvalidPage_DefaultsToPageOne()
        {
            // Arrange
            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((new List<Bundle>().AsEnumerable(), 0));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 0, 12);

            // Assert
            Assert.That(result.Page, Is.EqualTo(1));
            _bundleRepositoryMock.Verify(
                r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithInvalidPageSize_DefaultsToTwelve()
        {
            // Arrange
            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((new List<Bundle>().AsEnumerable(), 0));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 1, 0);

            // Assert
            Assert.That(result.PageSize, Is.EqualTo(12));
            _bundleRepositoryMock.Verify(
                r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12),
                Times.Once);
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithNullWords_ReturnsZeroWordCount()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "Empty Bundle",
                    DifficultyLevels = [],
                    Words = null!,
                    Categories = new List<Category>()
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.Items.First().WordCount, Is.EqualTo(0));
        }

        [Test]
        public async Task GetSystemBundlesAsync_WithNullCategories_ReturnsEmptyList()
        {
            // Arrange
            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Id = 1,
                    Title = "No Categories Bundle",
                    DifficultyLevels = [],
                    Words = new List<Word>(),
                    Categories = null!
                }
            };

            _bundleRepositoryMock
                .Setup(r => r.GetSystemBundlesPageAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync((bundles.AsEnumerable(), 1));

            // Act
            var result = await _bundleService.GetSystemBundlesAsync(null, null, null, null, null, 1, 12);

            // Assert
            Assert.That(result.Items.First().Categories, Is.Empty);
        }

        [Test]
        public async Task GetAllCategoriesAsync_ReturnsCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Fruits" },
                new Category { Id = 2, Name = "Technology" },
                new Category { Id = 3, Name = "Travel" }
            };

            _categoryRepositoryMock
                .Setup(r => r.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _bundleService.GetAllCategoriesAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Select(c => c.Name), Contains.Item("Fruits"));
            Assert.That(result.Select(c => c.Name), Contains.Item("Technology"));
            _categoryRepositoryMock.Verify(r => r.GetAllCategoriesAsync(), Times.Once);
        }
    }
}
