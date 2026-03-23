using Enrich.BLL.Interfaces;
using Enrich.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Enrich.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWordService, WordService>();

            return services;
        }
    }
}
