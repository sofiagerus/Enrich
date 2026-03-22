using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<ILogger<AccountController>> _loggerMock = null!;
        private Mock<IAuthService> _authServiceMock = null!;
        private Mock<IUserService> _userServiceMock = null!;

        private AccountController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AccountController>>();
            _authServiceMock = new Mock<IAuthService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new AccountController(
                _loggerMock.Object,
                _authServiceMock.Object,
                _userServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Signup_Get_ReturnsViewResult()
        {
            var result = _controller.Signup();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Signup_Post_WhenModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "invalid-email",
                Username = "Bo",
                Password = "123"
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Signup(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(model));
            Assert.That(_controller.ModelState.IsValid, Is.False);

            _authServiceMock.Verify(a => a.RegisterUserAsync(It.IsAny<UserSignupDTO>()), Times.Never);
        }

        [Test]
        public async Task Signup_Post_WhenRegistrationSucceeds_RedirectsToProfile()
        {
            var model = new SignupViewModel { Email = "test@test.com", Username = "Bohdan", Password = "StrongPassword123!" };
            _authServiceMock.Setup(a => a.RegisterUserAsync(It.IsAny<UserSignupDTO>())).ReturnsAsync(IdentityResult.Success);
            _authServiceMock.Setup(a => a.LoginAsync(It.IsAny<LoginDTO>())).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Signup(model);

            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Profile"));
        }

        [Test]
        public void Login_Get_ReturnsViewResult()
        {
            var result = _controller.Login();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_WhenAuthSucceeds_RedirectsToProfile()
        {
            var model = new LoginViewModel { Email = "test@test.com", Password = "Password123!" };
            _authServiceMock.Setup(a => a.LoginAsync(It.IsAny<LoginDTO>())).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Login(model);

            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Profile"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Account"));
        }

        [Test]
        public async Task Login_Post_WhenModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var model = new LoginViewModel { Email = "", Password = "" };
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(model));
            _authServiceMock.Verify(a => a.LoginAsync(It.IsAny<LoginDTO>()), Times.Never);
        }

        [Test]
        public async Task Logout_Post_RedirectsToLogin()
        {
            var result = await _controller.Logout();

            _authServiceMock.Verify(a => a.LogoutAsync(), Times.Once);
            var redirectResult = result as RedirectToActionResult;

            Assert.That(redirectResult!.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }
    }
}