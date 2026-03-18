using Enrich.BLL.Constants;
using Enrich.Web.Validators;
using Enrich.Web.ViewModels;
using NUnit.Framework;

namespace Enrich.UnitTests.Validators
{
    [TestFixture]
    public class SignupViewModelValidatorTests
    {
        private SignupViewModelValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new SignupViewModelValidator();
        }

        [Test]
        public void Validate_WhenModelIsValid_ReturnsTrue()
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = "ValidUser",
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [TestCase("", UserConstants.EmailRequired)]
        [TestCase("invalid-email", UserConstants.InvalidEmailFormat)]
        public void Validate_WhenEmailIsInvalid_ReturnsFalseWithExpectedError(string email, string expectedError)
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = email,
                Username = "ValidUser",
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e => e.PropertyName == "Email" && e.ErrorMessage == expectedError), Is.True);
        }

        [TestCase("", UserConstants.UsernameRequired)]
        [TestCase("ab", UserConstants.UsernameMinLengthMessage)]
        [TestCase("thisisaverylongusername17", UserConstants.UsernameMaxLengthMessage)]
        [TestCase("invalid username!", UserConstants.UsernameInvalidFormat)]
        public void Validate_WhenUsernameIsInvalid_ReturnsFalseWithExpectedError(string username, string expectedError)
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = username,
                Password = "ValidPassword123"
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e => e.PropertyName == "Username" && e.ErrorMessage == expectedError), Is.True);
        }

        [TestCase("", UserConstants.PasswordRequired)]
        [TestCase("short", UserConstants.PasswordMinLengthMessage)]
        [TestCase("nouppercase123", UserConstants.PasswordRequiresUppercase)]
        public void Validate_WhenPasswordIsInvalid_ReturnsFalseWithExpectedError(string password, string expectedError)
        {
            // Arrange
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = "ValidUser",
                Password = password
            };

            // Act
            var result = _validator.Validate(model);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(e => e.PropertyName == "Password" && e.ErrorMessage == expectedError), Is.True);
        }
    }
}
