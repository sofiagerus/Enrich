using System.Security.Claims;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class CollectionControllerTests
    {
        private Mock<ILogger<CollectionController>> _loggerMock;
        private Mock<IBundleService> _bundleServiceMock;
        private Mock<IOptions<PaginationSettings>> _paginationOptionsMock;
        private CollectionController _controller;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<CollectionController>>();
            _bundleServiceMock = new Mock<IBundleService>();
            _paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();

            _paginationOptionsMock.Setup(x => x.Value).Returns(new PaginationSettings { DefaultSystemBundlesPageSize = 12 });

            _controller = new CollectionController(
                _loggerMock.Object,
                _bundleServiceMock.Object,
                _paginationOptionsMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") };
            var identity = new ClaimsIdentity(claims, "mock");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Community_ReturnsViewResult_WithValidModel()
        {
            // Arrange
            var model = new SystemBundlesIndexViewModel { SearchTerm = "test search" };
            var pagedResult = new PagedResult<SystemBundleDTO>
            {
                Items = new List<SystemBundleDTO> { new SystemBundleDTO { Id = 1, Title = "Test Community Bundle" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 12
            };
            var categories = new List<Category> { new Category { Id = 1, Name = "Art" } };

            _bundleServiceMock
                .Setup(s => s.GetCommunityBundlesAsync("test search", null, null, null, null, 1, 12))
                .ReturnsAsync(pagedResult);

            _bundleServiceMock
                .Setup(s => s.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.Community(model, 1, 0);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var viewModel = viewResult.Model as SystemBundlesIndexViewModel;
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel.Bundles.TotalCount, Is.EqualTo(1));
            Assert.That(viewModel.Categories.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Community_AjaxRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var model = new SystemBundlesIndexViewModel();
            var pagedResult = new PagedResult<SystemBundleDTO>();

            _bundleServiceMock
                .Setup(s => s.GetCommunityBundlesAsync(null, null, null, null, null, 1, 12))
                .ReturnsAsync(pagedResult);

            _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

            // Act
            var result = await _controller.Community(model, 1, 0);

            // Assert
            var partialViewResult = result as PartialViewResult;
            Assert.That(partialViewResult, Is.Not.Null);
            Assert.That(partialViewResult.ViewName, Is.EqualTo("_BundleListPartial"));
        }
    }
}