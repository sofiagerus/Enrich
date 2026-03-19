using Enrich.BLL.Constants;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.Web.Controllers;
using Enrich.Web.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
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
        private Mock<IValidator<SignupViewModel>> _validatorMock = null!;
        private Mock<IValidator<LoginViewModel>> _loginValidatorMock = null!;
        private Mock<IValidator<UpdateProfileViewModel>> _profileValidatorMock = null!;
        private Mock<UserManager<User>> _userManagerMock = null!;
        private Mock<SignInManager<User>> _signInManagerMock = null!;
        private AccountController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AccountController>>();
            _authServiceMock = new Mock<IAuthService>();
            _userServiceMock = new Mock<IUserService>();
            _validatorMock = new Mock<IValidator<SignupViewModel>>();
            _loginValidatorMock = new Mock<IValidator<LoginViewModel>>();
            _profileValidatorMock = new Mock<IValidator<UpdateProfileViewModel>>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                null!,
                null!,
                null!,
                null!);
            _loggerMock = new Mock<ILogger<AccountController>>();

            _controller = new AccountController(
            _authServiceMock.Object,
            _userServiceMock.Object,
            _validatorMock.Object,
            _loginValidatorMock.Object,
            _profileValidatorMock.Object,
            _signInManagerMock.Object,
            _loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.TempData = tempData;
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _userManagerMock?.Object?.Dispose();
        }

        [Test]
        public void Signup_Get_ReturnsViewResult()
        {
            var result = _controller.Signup();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Signup_Post_WhenValidationFails_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "invalid-email",
                Username = "Bo",
                Password = "123"
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ("Email", UserConstants.InvalidEmailFormat)
            };

            var validationResult = new ValidationResult(validationFailures);

            _validatorMock
                .Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

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
            _validatorMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _authServiceMock.Setup(a => a.RegisterUserAsync(It.IsAny<UserSignupDTO>())).ReturnsAsync(IdentityResult.Success);

            var createdUser = new User { UserName = model.Username, Email = model.Email };
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(createdUser);
            _signInManagerMock.Setup(s => s.SignInAsync(createdUser, false, null)).Returns(Task.CompletedTask);

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

            _loginValidatorMock.Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Login(model);

            var redirectResult = result as RedirectToActionResult;

            Assert.That(redirectResult!.ActionName, Is.EqualTo("Profile"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Account"));
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