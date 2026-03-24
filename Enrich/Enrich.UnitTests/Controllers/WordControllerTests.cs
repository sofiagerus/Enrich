using System.Security.Claims;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class WordControllerTests
    {
        private const string FakeUserId = "user-test-id";

        private Mock<ILogger<WordController>> _loggerMock = null!;
        private Mock<IWordService> _wordServiceMock = null!;
        private Mock<IUserService> _userServiceMock = null!;
        private WordController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<WordController>>();
            _wordServiceMock = new Mock<IWordService>();
            _userServiceMock = new Mock<IUserService>();

            _controller = new WordController(
                _loggerMock.Object,
                _wordServiceMock.Object,
                _userServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, FakeUserId) },
                "TestAuth"));

            var httpContext = new DefaultHttpContext { User = user };

            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.TempData = tempData;

            _userServiceMock
                .Setup(u => u.GetCurrentUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(FakeUserId);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void MyWords_Get_ReturnsViewResult()
        {
            var result = _controller.MyWords();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Create_Get_ReturnsViewWithEmptyModel()
        {
            var result = _controller.Create();

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.InstanceOf<CreateWordViewModel>());
        }

        [Test]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var model = new CreateWordViewModel { Term = string.Empty };
            _controller.ModelState.AddModelError("Term", "Term is required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(model));
            _wordServiceMock.Verify(
                w => w.CreatePersonalWordAsync(
                    It.IsAny<string>(),
                    It.IsAny<CreatePersonalWordDTO>()),
                Times.Never);
        }

        [Test]
        public async Task Create_Post_WhenServiceSucceeds_RedirectsToMyWords()
        {
            // Arrange
            var model = new CreateWordViewModel { Term = "Serendipity" };
            _wordServiceMock
                .Setup(w => w.CreatePersonalWordAsync(FakeUserId, It.IsAny<CreatePersonalWordDTO>()))
                .ReturnsAsync((true, null));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo(nameof(_controller.MyWords)));
            var successMsg = _controller.TempData["SuccessMessage"];
            Assert.That(successMsg!.ToString(), Does.Contain("Serendipity"));
        }

        [Test]
        public async Task Create_Post_WhenServiceReturnsDuplicate_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateWordViewModel { Term = "Serendipity" };
            _wordServiceMock
                .Setup(w => w.CreatePersonalWordAsync(FakeUserId, It.IsAny<CreatePersonalWordDTO>()))
                .ReturnsAsync((false, "You already have a word 'Serendipity' in your personal dictionary."));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);
            var modelErrors = _controller.ModelState[string.Empty];
            Assert.That(
                modelErrors!.Errors[0].ErrorMessage,
                Does.Contain("Serendipity"));
        }

        [Test]
        public async Task Create_Post_CallsServiceWithCorrectDto()
        {
            // Arrange
            var model = new CreateWordViewModel
            {
                Term = "Ephemeral",
                Translation = "Тимчасовий",
                DifficultyLevel = "C1",
                PartOfSpeech = "adjective",
                Transcription = "/ɪˈfem(ə)r(ə)l/",
                Meaning = "Lasting for a very short time.",
                Example = "Fashions are ephemeral.",
            };

            _wordServiceMock
                .Setup(w => w.CreatePersonalWordAsync(FakeUserId, It.IsAny<CreatePersonalWordDTO>()))
                .ReturnsAsync((true, null));

            // Act
            await _controller.Create(model);

            // Assert
            _wordServiceMock.Verify(
                w => w.CreatePersonalWordAsync(
                    FakeUserId,
                    It.Is<CreatePersonalWordDTO>(dto =>
                        dto.Term == "Ephemeral" &&
                        dto.Translation == "Тимчасовий" &&
                        dto.DifficultyLevel == "C1" &&
                        dto.PartOfSpeech == "adjective")),
                Times.Once);
        }
    }
}
