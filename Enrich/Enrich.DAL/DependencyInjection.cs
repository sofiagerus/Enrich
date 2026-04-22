using Enrich.DAL.Data;
using Enrich.DAL.Interfaces;
using Enrich.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enrich.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDalServices(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBundleRepository, BundleRepository>();
            services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
            services.AddScoped<IWordProgressRepository, WordProgressRepository>();

            return services;
        }
    }
}
