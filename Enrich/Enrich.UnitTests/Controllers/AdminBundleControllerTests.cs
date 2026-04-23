using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Interfaces;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class AdminBundleControllerTests
    {
        private Mock<IBundleService> _bundleServiceMock = null!;
        private Mock<ICategoryRepository> _categoryRepositoryMock = null!;
        private Mock<IWordRepository> _wordRepositoryMock = null!;
        private AdminBundleController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleServiceMock = new Mock<IBundleService>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _wordRepositoryMock = new Mock<IWordRepository>();

            _controller = new AdminBundleController(
                _bundleServiceMock.Object,
                _categoryRepositoryMock.Object,
                _wordRepositoryMock.Object)
            {
                TempData = new TempDataDictionary(
                    new DefaultHttpContext(),
                    Mock.Of<ITempDataProvider>())
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithPagedSystemBundles()
        {
            // Arrange
            var pagedResult = new PagedResult<SystemBundleDTO>
            {
                Items = new List<SystemBundleDTO>
                {
                    new SystemBundleDTO { Id = 1, Title = "Sys Bundle" }
                },
                TotalCount = 1
            };

            _bundleServiceMock.Setup(s => s.GetSystemBundlesAsync(
                It.IsAny<string>(),
                null,
                null,
                null,
                null,
                1,
                20))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.Index(1, string.Empty);

            // Assert
            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = (ViewResult)result;

            Assert.That(
                viewResult.ViewData.Model,
                Is.InstanceOf<PagedResult<SystemBundleDTO>>());

            var model = (PagedResult<SystemBundleDTO>)viewResult.ViewData.Model!;
            Assert.That(model.Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new CreateBundleViewModel
            {
                Title = "New Sys Bundle"
            };

            _bundleServiceMock.Setup(s => s.CreateSystemBundleAsync(It.IsAny<CreateBundleDTO>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;

            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }
    }
}