using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _authService = new AuthService(_userManagerMock.Object);
        }

        [Test]
        public async Task RegisterUserAsync_WhenCalled_CreatesUserWithCorrectProperties()
        {
            // Arrange
            var dto = new UserSignupDTO
            {
                Email = "test@example.com",
                Username = "TestUser",
                Password = "Password123!"
            };

            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.True);
            
            _userManagerMock.Verify(u => u.CreateAsync(It.Is<User>(user => 
                user.UserName == dto.Email && 
                user.Email == dto.Email
            ), dto.Password), Times.Once);
        }

        [Test]
        public async Task RegisterUserAsync_WhenCreateAsyncFails_ReturnsFailedIdentityResult()
        {
            // Arrange
            var dto = new UserSignupDTO
            {
                Email = "fail@example.com",
                Username = "FailUser",
                Password = "Password123!"
            };

            var identityError = new IdentityError { Code = "Error", Description = "Some error" };
            var failedResult = IdentityResult.Failed(identityError);

            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<User>(), dto.Password))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _authService.RegisterUserAsync(dto);

            // Assert
            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Code, Is.EqualTo("Error"));
        }
    }
}
