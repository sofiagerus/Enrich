using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
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
            await SeedSystemBundlesAsync(scope.ServiceProvider);
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
                    Translation = "Sustainable",
                    Transcription = "rɪˈzɪliənt",
                    Meaning = "Able to withstand or recover quickly from difficult conditions.",
                    PartOfSpeech = "Adjective",
                    Example = "The community was resilient in the face of the disaster.",
                    DifficultyLevel = "B2",
                    IsGlobal = true,
                    Categories = new List<Category> { generalCat! },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new Word
                {
                    Term = "Algorithm",
                    Translation = "Algorithm",
                    Transcription = "ˈælɡərɪðəm",
                    Meaning = "A process or set of rules to be followed in calculations or other problem-solving operations, especially by a computer.",
                    PartOfSpeech = "Noun",
                    Example = "The social media algorithm determines what content you see.",
                    DifficultyLevel = "B1",
                    IsGlobal = true,
                    Categories = new List<Category> { techCat! },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new Word
                {
                    Term = "Biodiversity",
                    Translation = "Biodiversity",
                    Transcription = "ˌbaɪəʊdaɪˈvɜːsəti",
                    Meaning = "The variety of plant and animal life in the world or in a particular habitat.",
                    PartOfSpeech = "Noun",
                    Example = "The rainforest is a hotspot of biodiversity.",
                    DifficultyLevel = "C1",
                    IsGlobal = true,
                    Categories = new List<Category> { natureCat! },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new Word
                {
                    Term = "Obsolete",
                    Translation = "Obsolete",
                    Transcription = "ˈɒbsəliːt",
                    Meaning = "No longer produced or used; out of date.",
                    PartOfSpeech = "Adjective",
                    Example = "Floppy disks are now obsolete.",
                    DifficultyLevel = "B2",
                    IsGlobal = true,
                    Categories = new List<Category> { techCat!, generalCat! },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                },
                new Word
                {
                    Term = "Mitigate",
                    Translation = "Mitigate",
                    Transcription = "ˈmɪtɪɡeɪt",
                    Meaning = "Make (something bad) less severe, serious, or painful.",
                    PartOfSpeech = "Verb",
                    Example = "Drainage schemes have helped to mitigate this problem.",
                    DifficultyLevel = "C1",
                    IsGlobal = true,
                    Categories = new List<Category> { generalCat! },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
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
                "Travel", "Food", "Nature", "Education", "Finance",
                "Fruits", "Emotions", "Housing", "Healthcare"
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

        private static async Task SeedSystemBundlesAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            if (await db.Bundles.AnyAsync(b => b.IsSystem))
            {
                return;
            }

            var categories = await db.Categories.ToListAsync();
            var words = await db.Words.Where(w => w.IsGlobal).ToListAsync();

            var fruitsCat = categories.FirstOrDefault(c => c.Name == "Fruits");
            var foodCat = categories.FirstOrDefault(c => c.Name == "Food");
            var travelCat = categories.FirstOrDefault(c => c.Name == "Travel");
            var financeCat = categories.FirstOrDefault(c => c.Name == "Finance");
            var housingCat = categories.FirstOrDefault(c => c.Name == "Housing");
            var healthcareCat = categories.FirstOrDefault(c => c.Name == "Healthcare");
            var emotionsCat = categories.FirstOrDefault(c => c.Name == "Emotions");
            var techCat = categories.FirstOrDefault(c => c.Name == "Technology");
            var natureCat = categories.FirstOrDefault(c => c.Name == "Nature");
            var generalCat = categories.FirstOrDefault(c => c.Name == "General");

            var now = DateTime.UtcNow;

            var bundles = new List<Bundle>
            {
                new Bundle
                {
                    Title = "Fruits",
                    Description = "Essential fruit vocabulary for daily life. Perfect for navigating supermarkets, reading recipes, or discussing healthy eating habits.",
                    DifficultyLevels = ["C2", "C1", "B2", "B1", "A2", "A1"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { fruitsCat!, foodCat! }.Where(c => c != null).ToList(),
                    Words = words.Take(2).ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Bundle
                {
                    Title = "Living Abroad Survival Kit",
                    Description = "Moving to an English-speaking country? This ultimate bundle covers everything you need.",
                    DifficultyLevels = ["C2", "C1", "B2", "B1"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { housingCat!, healthcareCat!, financeCat! }.Where(c => c != null).ToList(),
                    Words = words.Take(3).ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Bundle
                {
                    Title = "Tech Terminology",
                    Description = "Master the language of technology. From software development to hardware components, this bundle has you covered.",
                    DifficultyLevels = ["B2", "B1", "A2"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { techCat! }.Where(c => c != null).ToList(),
                    Words = words.Where(w => w.Term == "Algorithm" || w.Term == "Obsolete").ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Bundle
                {
                    Title = "Nature & Environment",
                    Description = "Vocabulary related to nature, wildlife, and environmental topics. Great for discussing sustainability and ecology.",
                    DifficultyLevels = ["C1", "B2", "B1"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { natureCat! }.Where(c => c != null).ToList(),
                    Words = words.Where(w => w.Term == "Biodiversity" || w.Term == "Mitigate").ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Bundle
                {
                    Title = "Travel Essentials",
                    Description = "Everything you need to navigate airports, hotels, and tourist destinations. Perfect for your next adventure.",
                    DifficultyLevels = ["B1", "A2", "A1"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { travelCat! }.Where(c => c != null).ToList(),
                    Words = words.Take(1).ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Bundle
                {
                    Title = "Emotions & Feelings",
                    Description = "Express yourself better with this comprehensive vocabulary set covering emotions, moods, and mental states.",
                    DifficultyLevels = ["C2", "C1", "B2"],
                    IsSystem = true,
                    IsPublic = true,
                    Status = BundleStatus.Published,
                    Categories = new List<Category> { emotionsCat!, generalCat! }.Where(c => c != null).ToList(),
                    Words = words.Where(w => w.Term == "Resilient").ToList(),
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            db.Bundles.AddRange(bundles);
            await db.SaveChangesAsync();
        }
    }
}