using Enrich.BLL.Interfaces;
using Enrich.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Enrich.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllServices(this IServiceCollection services)
        {
            // services.AddScoped<IService, Service>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
