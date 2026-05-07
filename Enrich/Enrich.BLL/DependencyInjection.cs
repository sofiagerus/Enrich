using Enrich.BLL.Clients;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Services;
using Enrich.BLL.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enrich.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBllServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<PaginationSettings>(
                configuration.GetSection(PaginationSettings.Section));

            services.Configure<CacheSettings>(
                configuration.GetSection(CacheSettings.Section));

            // Register Typed Client
            services.AddHttpClient<IDictionaryApiClient, DictionaryApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.dictionaryapi.dev/api/v2/entries/en/");
            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWordService, WordService>();
            services.AddScoped<IBundleService, BundleService>();
            services.AddScoped<IStudySessionService, StudySessionService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
