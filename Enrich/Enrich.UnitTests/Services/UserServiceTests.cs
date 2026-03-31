using Enrich.BLL.DTOs;
using Enrich.BLL.Services;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Enrich.UnitTests.Services
{
    [TestFixture]
    public class UserServiceTests : ServiceTestBase
    {
        private Mock<ILogger<UserService>> _loggerMock = null!;
        private UserService _userService = null!;

        [SetUp]
        public void SetUp()
        {
            SetUpIdentityMocks();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(UserManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task UpdateProfileAsync_WithValidUser_UpdatesSuccessfully()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "oldName", Bio = "oldBio" };
            var profileDto = new UpdateProfileDTO { Username = "newName", Bio = "newBio" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            UserManagerMock.Setup(m => m.SetUserNameAsync(existingUser, profileDto.Username))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, name) => u.UserName = name);

            UserManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateProfileAsync(userId, profileDto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(existingUser.UserName, Is.EqualTo("newName"));
            Assert.That(existingUser.Bio, Is.EqualTo("newBio"));
            UserManagerMock.Verify(m => m.UpdateAsync(existingUser), Times.Once);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenUserNotFound_ReturnsFailedResult()
        {
            // Arrange
            var userId = "non-existent-user-id";
            var profileDto = new UpdateProfileDTO { Username = "newName", Bio = "newBio" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateProfileAsync(userId, profileDto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Користувача не знайдено."));
            UserManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenUsernameIsTaken_ReturnsFailedResult()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "oldName" };
            var profileDto = new UpdateProfileDTO { Username = "alreadyTaken" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            UserManagerMock.Setup(m => m.SetUserNameAsync(existingUser, "alreadyTaken"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Username is already taken." }));

            // Act
            var result = await _userService.UpdateProfileAsync(userId, profileDto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("taken"));
            UserManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenFinalUpdateFails_ReturnsFailedResult()
        {
            // Arrange
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "name" };
            var profileDto = new UpdateProfileDTO { Username = "name", Bio = "new bio" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            UserManagerMock.Setup(m => m.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Database error." }));

            // Act
            var result = await _userService.UpdateProfileAsync(userId, profileDto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Database error."));
        }

        [Test]
        public async Task RestrictUserAsync_WithValidUser_RestrictsSuccessfully()
        {
            // Arrange
            var userId = "test-user-id";
            var dto = new RestrictAccountDTO { UserId = userId, Reason = "Порушення правил", LockoutDays = 36500 };
            var existingUser = new User { Id = userId, UserName = "bad_user" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            UserManagerMock.Setup(m => m.SetLockoutEndDateAsync(existingUser, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.RestrictUserAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            UserManagerMock.Verify(m => m.SetLockoutEndDateAsync(existingUser, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Test]
        public async Task RestrictUserAsync_WhenUserNotFound_ReturnsFailedResult()
        {
            // Arrange
            var userId = "non-existent-user-id";
            var dto = new RestrictAccountDTO { UserId = userId, Reason = "Спам" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RestrictUserAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Користувача не знайдено."));
            UserManagerMock.Verify(m => m.SetLockoutEndDateAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>()), Times.Never);
        }

        [Test]
        public async Task RestoreUserAsync_WithValidUser_RestoresSuccessfully()
        {
            // Arrange
            var userId = "test-user-id";
            var dto = new RestoreAccountDTO { UserId = userId };
            var existingUser = new User { Id = userId, UserName = "restored_user" };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            UserManagerMock.Setup(m => m.SetLockoutEndDateAsync(existingUser, null))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.RestoreUserAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            UserManagerMock.Verify(m => m.SetLockoutEndDateAsync(existingUser, null), Times.Once);
        }

        [Test]
        public async Task RestoreUserAsync_WhenUserNotFound_ReturnsFailedResult()
        {
            // Arrange
            var userId = "non-existent-id";
            var dto = new RestoreAccountDTO { UserId = userId };

            UserManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RestoreUserAsync(dto);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Користувача не знайдено."));
        }
    }
}