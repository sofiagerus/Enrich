using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Enrich.Web.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@enrich.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = "SuperAdmin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createPowerUser = await userManager.CreateAsync(newAdmin, "AdminPassword123!");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            await SeedCategoriesAsync(scope.ServiceProvider);
            await SeedGlobalWordsAsync(scope.ServiceProvider);
        }

        private static async Task SeedGlobalWordsAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            if (await db.Words.AnyAsync(w => w.IsGlobal))
            {
                return;
            }

            var categories = await db.Categories.ToListAsync();
            var generalCat = categories.FirstOrDefault(c => c.Name == "General");
            var techCat = categories.FirstOrDefault(c => c.Name == "Technology");
            var natureCat = categories.FirstOrDefault(c => c.Name == "Nature");

            var globalWords = new List<Word>
            {
                new Word
                {
                    Term = "Resilient",
                    Translation = "Стійкий",
                    Transcription = "rɪˈzɪliənt",
                    Meaning = "Able to withstand or recover quickly from difficult conditions.",
                    PartOfSpeech = "Adjective",
                    Example = "The community was resilient in the face of the disaster.",
                    DifficultyLevel = "B2",
                    IsGlobal = true,
                    Categories = new List<Category> { generalCat! },
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                },
                new Word
                {
                    Term = "Algorithm",
                    Translation = "Алгоритм",
                    Transcription = "ˈælɡərɪðəm",
                    Meaning = "A process or set of rules to be followed in calculations or other problem-solving operations, especially by a computer.",
                    PartOfSpeech = "Noun",
                    Example = "The social media algorithm determines what content you see.",
                    DifficultyLevel = "B1",
                    IsGlobal = true,
                    Categories = new List<Category> { techCat! },
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                },
                new Word
                {
                    Term = "Biodiversity",
                    Translation = "Біорізноманіття",
                    Transcription = "ˌbaɪəʊdaɪˈvɜːsəti",
                    Meaning = "The variety of plant and animal life in the world or in a particular habitat.",
                    PartOfSpeech = "Noun",
                    Example = "The rainforest is a hotspot of biodiversity.",
                    DifficultyLevel = "C1",
                    IsGlobal = true,
                    Categories = new List<Category> { natureCat! },
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                },
                new Word
                {
                    Term = "Obsolete",
                    Translation = "Застарілий",
                    Transcription = "ˈɒbsəliːt",
                    Meaning = "No longer produced or used; out of date.",
                    PartOfSpeech = "Adjective",
                    Example = "Floppy disks are now obsolete.",
                    DifficultyLevel = "B2",
                    IsGlobal = true,
                    Categories = new List<Category> { techCat!, generalCat! },
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                },
                new Word
                {
                    Term = "Mitigate",
                    Translation = "Пом'якшувати",
                    Transcription = "ˈmɪtɪɡeɪt",
                    Meaning = "Make (something bad) less severe, serious, or painful.",
                    PartOfSpeech = "Verb",
                    Example = "Drainage schemes have helped to mitigate this problem.",
                    DifficultyLevel = "C1",
                    IsGlobal = true,
                    Categories = new List<Category> { generalCat! },
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                }
            };

            db.Words.AddRange(globalWords);

            await db.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            var defaultCategories = new[]
            {
                "General", "Business", "Technology", "Science", "Medicine",
                "Law", "Politics", "Art", "Music", "Sports",
                "Travel", "Food", "Nature", "Education", "Finance"
            };

            foreach (var name in defaultCategories)
            {
                var exists = await db.Categories.AnyAsync(c => c.Name == name);
                if (!exists)
                {
                    db.Categories.Add(new Category { Name = name });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}