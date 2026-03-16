using Enrich.BLL;
using Enrich.DAL;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Запуск веб-хоста...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Register Services
    builder.Services.AddDalServices(builder.Configuration);
    builder.Services.AddBllServices();
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    Log.Information("Додаток Enrich запущено");

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapControllerRoute(
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