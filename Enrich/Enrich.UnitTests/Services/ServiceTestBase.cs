using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Enrich.UnitTests.Services
{
    public abstract class ServiceTestBase
    {
        protected Mock<UserManager<User>> UserManagerMock { get; private set; } = null!;

        protected Mock<SignInManager<User>> SignInManagerMock { get; private set; } = null!;

        protected void SetUpIdentityMocks()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            UserManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();

            SignInManagerMock = new Mock<SignInManager<User>>(
                UserManagerMock.Object,
                contextAccessorMock.Object,
                claimsFactoryMock.Object,
                null!, null!, null!, null!);
        }
    }
}
