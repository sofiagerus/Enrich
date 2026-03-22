using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;
using Enrich.Web.ViewModels;
using NUnit.Framework;

namespace Enrich.UnitTests.Validators
{
    [TestFixture]
    public class SignupViewModelTests
    {
        private static List<ValidationResult> ValidateModel(SignupViewModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Test]
        public void Validate_WhenModelIsValid_NoErrors()
        {
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = "ValidUser",
                Password = "ValidPassword123"
            };

            var results = ValidateModel(model);

            Assert.That(results, Is.Empty);
        }

        [TestCase("", UserConstants.EmailRequired)]
        [TestCase("invalid-email", UserConstants.InvalidEmailFormat)]
        public void Validate_WhenEmailIsInvalid_ReturnsExpectedError(string email, string expectedError)
        {
            var model = new SignupViewModel
            {
                Email = email,
                Username = "ValidUser",
                Password = "ValidPassword123"
            };

            var results = ValidateModel(model);

            Assert.That(results.Any(r => r.MemberNames.Contains("Email") && r.ErrorMessage == expectedError), Is.True);
        }

        [TestCase("", UserConstants.UsernameRequired)]
        [TestCase("ab", UserConstants.UsernameMinLengthMessage)]
        [TestCase("thisisaverylongusername17", UserConstants.UsernameMaxLengthMessage)]
        [TestCase("invalid username!", UserConstants.UsernameInvalidFormat)]
        public void Validate_WhenUsernameIsInvalid_ReturnsExpectedError(string username, string expectedError)
        {
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = username,
                Password = "ValidPassword123"
            };

            var results = ValidateModel(model);

            Assert.That(results.Any(r => r.MemberNames.Contains("Username") && r.ErrorMessage == expectedError), Is.True);
        }

        [TestCase("", UserConstants.PasswordRequired)]
        [TestCase("short", UserConstants.PasswordMinLengthMessage)]
        public void Validate_WhenPasswordIsInvalid_ReturnsExpectedError(string password, string expectedError)
        {
            var model = new SignupViewModel
            {
                Email = "test@example.com",
                Username = "ValidUser",
                Password = password
            };

            var results = ValidateModel(model);

            Assert.That(results.Any(r => r.MemberNames.Contains("Password") && r.ErrorMessage == expectedError), Is.True);
        }
    }
}
