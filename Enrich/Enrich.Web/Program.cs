using Enrich.BLL;
using Enrich.DAL;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск веб-хоста...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    _ = builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Register Services
    _ = builder.Services.AddDalServices(builder.Configuration);
    _ = builder.Services.AddBllServices();
    _ = builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");
    _ = builder.Services.AddControllersWithViews()
        .AddViewLocalization();

    WebApplication app = builder.Build();

    Log.Information("Додаток Enrich запущено");

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        _ = app.UseExceptionHandler("/Home/Error");
        _ = app.UseHsts();
    }

    _ = app.UseHttpsRedirection();

    string[] supportedCultures = ["uk", "en"];

    _ = app.UseRequestLocalization(options =>
    {
        _ = options.SetDefaultCulture("uk")
               .AddSupportedCultures(supportedCultures)
               .AddSupportedUICultures(supportedCultures);
    });

    _ = app.UseRouting();
    _ = app.UseAuthorization();

    _ = app.MapStaticAssets();
    _ = app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

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