using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class AuthServiceTests : ServiceTestBase
    {
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            SetUpIdentityMocks();
            _authService = new AuthService(UserManagerMock.Object, SignInManagerMock.Object);
        }

        [Test]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var dto = new LoginDTO { Email = "test@test.com", Password = "Password123!", RememberMe = false };
            var user = new User { UserName = "TestUser", Email = dto.Email };

            UserManagerMock
                .Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            SignInManagerMock
                .Setup(s => s.PasswordSignInAsync(user.UserName, dto.Password, dto.RememberMe, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            SignInManagerMock.Verify(s => s.PasswordSignInAsync(user.UserName, dto.Password, dto.RememberMe, false), Times.Once);
        }

        [Test]
        public async Task LoginAsync_WithInvalidEmail_ReturnsFailed()
        {
            // Arrange
            var dto = new LoginDTO { Email = "wrong@test.com", Password = "Password123!", RememberMe = false };

            UserManagerMock
                .Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync((User)null!);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.Not.Null);
        }

        [Test]
        public async Task LogoutAsync_WhenCalled_InvokesSignOut()
        {
            // Act
            await _authService.LogoutAsync();

            // Assert
            SignInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Test]
        public async Task RegisterUserAsync_WhenCalled_CreatesUserWithCorrectProperties()
        {
            // Arrange
            var dto = new UserSignupDTO { Email = "test@example.com", Username = "TestUser", Password = "Password123!" };

            UserManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _authService.RegisterUserAsync(dto);

            // Assert
            UserManagerMock.Verify(u => u.CreateAsync(It.Is<User>(user => user.UserName == dto.Username && user.Email == dto.Email), dto.Password), Times.Once);
        }
    }
}