using Enrich.BLL;
using Enrich.DAL;
using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.Web.Handlers;
using Enrich.Web.Middlewares;
using Enrich.Web.Seeders;
using Enrich.Web.Settings;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск веб-хоста...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsStaging() || builder.Environment.IsProduction())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.Configure<IdentitySettings>(
        builder.Configuration.GetSection(IdentitySettings.Section));

    builder.Services.Configure<LocalizationSettings>(
        builder.Configuration.GetSection(LocalizationSettings.Section));

    var identitySettings = builder.Configuration
        .GetSection(IdentitySettings.Section)
        .Get<IdentitySettings>() ?? new IdentitySettings();

    // Register Services
    builder.Services.AddMemoryCache();
    builder.Services.AddDalServices(builder.Configuration);
    builder.Services.AddBllServices(builder.Configuration);
    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = identitySettings.RequireDigit;
        options.Password.RequiredLength = identitySettings.RequiredLength;
        options.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;
        options.Lockout.AllowedForNewUsers = identitySettings.LockoutAllowedForNewUsers;
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    {
        options.ValidationInterval = TimeSpan.Zero;
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        })
        .AddViewLocalization();

    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
    });

    WebApplication app = builder.Build();

    await DataSeeder.SeedRolesAndAdminAsync(app.Services);

    var envLabel = app.Configuration["Environment:Label"] ?? "unknown";
    Log.Information(
        "Середовище: {EnvironmentLabel}",
        envLabel);

    Log.Information("Додаток Enrich запущено");

    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

    // Configure the HTTP request pipeline
    app.UseMiddleware<ExecutionTimeMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    var localizationSettings = app.Configuration
        .GetSection(LocalizationSettings.Section)
        .Get<LocalizationSettings>() ?? new LocalizationSettings();

    app.UseRequestLocalization(options =>
    {
        options.SetDefaultCulture(localizationSettings.DefaultCulture)
               .AddSupportedCultures(localizationSettings.SupportedCultures)
               .AddSupportedUICultures(localizationSettings.SupportedCultures);
    });

    app.UseRouting();
    app.UseAntiforgery();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();
    }
    else
    {
        app.UseStaticFiles();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Додаток завершився критичною помилкою під час запуску");
}
finally
{
    Log.CloseAndFlush();
}