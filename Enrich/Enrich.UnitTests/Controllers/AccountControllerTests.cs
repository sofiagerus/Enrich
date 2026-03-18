using System.Security.Claims;
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
        private Mock<IValidator<SignupViewModel>> _validatorMock = null!;
        private Mock<UserManager<User>> _userManagerMock = null!;
        private Mock<SignInManager<User>> _signInManagerMock = null!;
        private AccountController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<AccountController>>();
            _authServiceMock = new Mock<IAuthService>();
            _validatorMock = new Mock<IValidator<SignupViewModel>>();

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

            _controller = new AccountController(
                _loggerMock.Object,
                _authServiceMock.Object,
                _validatorMock.Object,
                _signInManagerMock.Object);
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
            // Act
            var result = _controller.Signup();

            // Assert
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
                new ValidationFailure("Email", UserConstants.InvalidEmailFormat)
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
            // Arrange
            var model = new SignupViewModel
            {
                Email = "test@test.com",
                Username = "Bohdan",
                Password = "StrongPassword123!"
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _authServiceMock
                .Setup(a => a.RegisterUserAsync(It.IsAny<UserSignupDTO>()))
                .ReturnsAsync(IdentityResult.Success);

            var createdUser = new User { UserName = model.Username, Email = model.Email };

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(model.Email))
                .ReturnsAsync(createdUser);

            _signInManagerMock
                .Setup(s => s.SignInAsync(createdUser, false, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Signup(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Profile"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Account"));
        }

        [Test]
        public async Task Signup_Post_WhenRegistrationFails_AddsErrorsToModelState()
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "test@test.com",
                Username = "Bohdan",
                Password = "password"
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Username taken" };
            var failedResult = IdentityResult.Failed(identityError);

            _authServiceMock
                .Setup(a => a.RegisterUserAsync(It.IsAny<UserSignupDTO>()))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.Signup(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);

            var hasError = _controller.ModelState.Values.Any(v => v.Errors.Any(e => e.ErrorMessage == "Username taken"));
            Assert.That(hasError, Is.True);
        }
    }
}
