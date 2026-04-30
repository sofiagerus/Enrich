using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities.Enums;
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
        private Mock<IWordService> _wordServiceMock = null!;
        private AdminBundleController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _bundleServiceMock = new Mock<IBundleService>();
            _wordServiceMock = new Mock<IWordService>();

            _controller = new AdminBundleController(
                _bundleServiceMock.Object,
                _wordServiceMock.Object)
            {
                // Тут TempData вже ініціалізована правильно для всіх тестів у цьому класі
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
                It.IsAny<string>(), null, null, null, null, 1, 20))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.Index(1, string.Empty);

            // Assert
            Assert.That(result, Is.TypeOf<ViewResult>());
            var viewResult = (ViewResult)result;

            Assert.That(viewResult.ViewData.Model, Is.InstanceOf<PagedResult<SystemBundleDTO>>());

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
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;

            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var model = new EditBundleViewModel { Id = 2 };

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_Post_SystemBundle_CallsUpdateSystemAndRedirectsToIndex()
        {
            // Arrange
            var model = new EditBundleViewModel { Id = 1, IsSystem = true, Title = "Sys" };

            _bundleServiceMock.Setup(s => s.UpdateSystemBundleAsync(1, It.IsAny<CreateBundleDTO>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            _bundleServiceMock.Verify(s => s.UpdateSystemBundleAsync(1, It.IsAny<CreateBundleDTO>()), Times.Once);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Edit_Post_CommunityBundle_CallsUpdateCommunityAndRedirectsToCommunity()
        {
            // Arrange
            var model = new EditBundleViewModel { Id = 1, IsSystem = false, Title = "Comm", Status = BundleStatus.Published };

            _bundleServiceMock.Setup(s => s.UpdateCommunityBundleAsync(1, It.IsAny<CreateBundleDTO>(), BundleStatus.Published))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            _bundleServiceMock.Verify(s => s.UpdateCommunityBundleAsync(1, It.IsAny<CreateBundleDTO>(), BundleStatus.Published), Times.Once);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Community"));
        }
    }
}