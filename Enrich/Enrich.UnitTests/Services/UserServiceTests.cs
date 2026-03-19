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
    public class UserServiceTests
    {
        private Mock<UserManager<User>> userManagerMock = null!;
        private Mock<ILogger<UserService>> loggerMock = null!;
        private UserService userService = null!;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            loggerMock = new Mock<ILogger<UserService>>();

            userService = new UserService(userManagerMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task UpdateProfileAsync_WithValidUser_UpdatesSuccessfully()
        {
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "oldName", Bio = "oldBio" };
            var profileDto = new UpdateProfileDTO { Username = "newName", Bio = "newBio" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            userManagerMock.Setup(m => m.SetUserNameAsync(existingUser, profileDto.Username))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, name) => u.UserName = name);

            userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await userService.UpdateProfileAsync(userId, profileDto);

            Assert.That(result.Succeeded, Is.True);
            Assert.That(existingUser.UserName, Is.EqualTo("newName"));
            Assert.That(existingUser.Bio, Is.EqualTo("newBio"));
            userManagerMock.Verify(m => m.UpdateAsync(existingUser), Times.Once);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenUserNotFound_ReturnsFailedResult()
        {
            var userId = "non-existent-user-id";
            var profileDto = new UpdateProfileDTO { Username = "newName", Bio = "newBio" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);

            var result = await userService.UpdateProfileAsync(userId, profileDto);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Description, Is.EqualTo("Користувача не знайдено."));
            userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenUsernameIsTaken_ReturnsFailedResult()
        {
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "oldName" };
            var profileDto = new UpdateProfileDTO { Username = "alreadyTaken" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            userManagerMock.Setup(m => m.SetUserNameAsync(existingUser, "alreadyTaken"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Username is already taken." }));

            var result = await userService.UpdateProfileAsync(userId, profileDto);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.Any(e => e.Description.Contains("taken")), Is.True);
            userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public async Task UpdateProfileAsync_WhenFinalUpdateFails_ReturnsFailedResult()
        {
            var userId = "test-user-id";
            var existingUser = new User { Id = userId, UserName = "name" };
            var profileDto = new UpdateProfileDTO { Username = "name", Bio = "new bio" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            userManagerMock.Setup(m => m.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Database error." }));

            var result = await userService.UpdateProfileAsync(userId, profileDto);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Description, Is.EqualTo("Database error."));
        }

        [Test]
        public async Task RestrictUserAsync_WithValidUser_RestrictsSuccessfully()
        {
            var userId = "test-user-id";
            var dto = new RestrictAccountDTO { UserId = userId, Reason = "Порушення правил", LockoutDays = 36500 };
            var existingUser = new User { Id = userId, UserName = "bad_user" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(existingUser);

            userManagerMock.Setup(m => m.SetLockoutEndDateAsync(existingUser, It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await userService.RestrictUserAsync(dto);

            Assert.That(result.Succeeded, Is.True);
            userManagerMock.Verify(m => m.SetLockoutEndDateAsync(existingUser, It.IsAny<DateTimeOffset>()), Times.Once);
        }

        [Test]
        public async Task RestrictUserAsync_WhenUserNotFound_ReturnsFailedResult()
        {
            var userId = "non-existent-user-id";
            var dto = new RestrictAccountDTO { UserId = userId, Reason = "Спам" };

            userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User?)null);

            var result = await userService.RestrictUserAsync(dto);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Errors.First().Description, Is.EqualTo("Користувача не знайдено."));
            userManagerMock.Verify(m => m.SetLockoutEndDateAsync(It.IsAny<User>(), It.IsAny<DateTimeOffset>()), Times.Never);
        }
    }
}