using Enrich.BLL.Common;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class AdminControllerTests
    {
        private Mock<IUserService> _userServiceMock = null!;
        private Mock<IBundleService> _bundleServiceMock = null!;
        private Mock<IOptions<PaginationSettings>> _paginationOptionsMock = null!;
        private Mock<ILogger<AdminController>> _loggerMock = null!;
        private AdminController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();
            _bundleServiceMock = new Mock<IBundleService>();

            _paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();
            _paginationOptionsMock.Setup(o => o.Value).Returns(new PaginationSettings { DefaultSystemBundlesPageSize = 12 });

            _loggerMock = new Mock<ILogger<AdminController>>();

            _controller = new AdminController(
                _userServiceMock.Object,
                _bundleServiceMock.Object,
                _paginationOptionsMock.Object,
                _loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new Mock<ITempDataDictionary>().Object;
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task ReviewBundle_InvalidAction_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ReviewBundle(1, "invalid_action");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);

            var value = badRequestResult!.Value;
            var messageProp = value?.GetType().GetProperty("message")?.GetValue(value, null) as string;
            Assert.That(messageProp, Is.EqualTo("Invalid action."));
            _bundleServiceMock.Verify(s => s.ReviewBundleAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task ReviewBundle_PublishSuccess_ReturnsOk()
        {
            // Arrange
            _bundleServiceMock.Setup(s => s.ReviewBundleAsync(1, true)).ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.ReviewBundle(1, "publish");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var value = okResult!.Value;
            var messageProp = value?.GetType().GetProperty("message")?.GetValue(value, null) as string;
            Assert.That(messageProp, Is.EqualTo("Bundle successfully published!"));
            _bundleServiceMock.Verify(s => s.ReviewBundleAsync(1, true), Times.Once);
        }

        [Test]
        public async Task ReviewBundle_RejectSuccess_ReturnsOk()
        {
            // Arrange
            _bundleServiceMock.Setup(s => s.ReviewBundleAsync(1, false)).ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.ReviewBundle(1, "reject");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var value = okResult!.Value;
            var messageProp = value?.GetType().GetProperty("message")?.GetValue(value, null) as string;
            Assert.That(messageProp, Is.EqualTo("Bundle successfully rejected!"));
            _bundleServiceMock.Verify(s => s.ReviewBundleAsync(1, false), Times.Once);
        }

        [Test]
        public async Task ReviewBundle_ServiceFails_ReturnsBadRequest()
        {
            // Arrange
            _bundleServiceMock.Setup(s => s.ReviewBundleAsync(1, true)).ReturnsAsync(Result.Failure("Error msg"));

            // Act
            var result = await _controller.ReviewBundle(1, "publish");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);

            var value = badRequestResult!.Value;
            var messageProp = value?.GetType().GetProperty("message")?.GetValue(value, null) as string;
            Assert.That(messageProp, Is.EqualTo("Error msg"));
            _bundleServiceMock.Verify(s => s.ReviewBundleAsync(1, true), Times.Once);
        }
    }
}