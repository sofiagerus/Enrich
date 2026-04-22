using System.Security.Claims;
using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class AdminWordControllerTests
    {
        private Mock<IWordService> _wordServiceMock = null!;
        private Mock<IOptions<PaginationSettings>> _paginationMock = null!;
        private AdminWordController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _wordServiceMock = new Mock<IWordService>();

            _paginationMock = new Mock<IOptions<PaginationSettings>>();
            _paginationMock.Setup(p => p.Value).Returns(new PaginationSettings { DefaultSystemWordsPageSize = 25 });

            _controller = new AdminWordController(
                _wordServiceMock.Object,
                _paginationMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user-1"),
                },
                "mock"));

            var httpContext = new DefaultHttpContext() { User = user };

            var tempDataProvider = new Mock<ITempDataProvider>();
            _controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewWithWords()
        {
            // Arrange
            var pageResult = new PagedResult<SystemWordDTO>
            {
                Items = new List<SystemWordDTO> { new SystemWordDTO { Term = "Test Word" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 25
            };

            _wordServiceMock
                .Setup(s => s.GetSystemWordsAsync(It.IsAny<string>(), null, null, null, null, 1, 25))
                .ReturnsAsync(pageResult);
            _wordServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());

            var model = new SystemWordsIndexViewModel();

            // Act
            var result = await _controller.Index(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var returnedModel = viewResult!.Model as SystemWordsIndexViewModel;
            Assert.That(returnedModel, Is.Not.Null);
            Assert.That(returnedModel!.Words.Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new CreateWordViewModel { Term = "NewWord" };
            _wordServiceMock.Setup(s => s.CreateSystemWordAsync(It.IsAny<CreateSystemWordDTO>())).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null);
        }

        [Test]
        public async Task Create_Post_ServiceError_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateWordViewModel { Term = "DuplicateWord" };
            _wordServiceMock.Setup(s => s.CreateSystemWordAsync(It.IsAny<CreateSystemWordDTO>())).ReturnsAsync("Duplicate term");
            _wordServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(model));
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            // Arrange
            var wordId = 1;
            var word = new Word { Id = wordId, Term = "ExistingWord" };
            _wordServiceMock.Setup(s => s.GetSystemWordForEditAsync(wordId)).ReturnsAsync(word);
            _wordServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(wordId);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var model = viewResult!.Model as EditSystemWordViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Term, Is.EqualTo("ExistingWord"));
        }

        [Test]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var wordId = 999;
            Result<Word> failedResult = "Word not found";
            _wordServiceMock.Setup(s => s.GetSystemWordForEditAsync(wordId)).ReturnsAsync(failedResult);

            // Act
            var result = await _controller.Edit(wordId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var wordId = 1;
            var model = new EditSystemWordViewModel { Id = wordId, Term = "UpdatedWord" };
            _wordServiceMock.Setup(s => s.UpdateSystemWordAsync(wordId, It.IsAny<UpdateSystemWordDTO>())).ReturnsAsync(true);

            // Act
            var result = await _controller.Edit(wordId, model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null);
        }

        [Test]
        public async Task Delete_Post_ValidId_RedirectsToIndex()
        {
            // Arrange
            var wordId = 1;
            _wordServiceMock.Setup(s => s.DeleteSystemWordAsync(wordId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(wordId);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["SuccessMessage"], Is.Not.Null);
        }
    }
}
