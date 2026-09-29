using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Services.Interfaces;
using AttendanceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Récupération de la chaîne de connexion (Render injecte automatiquement les Env Vars dans Configuration)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// Configuration PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
});

// Injection de dépendances
builder.Services.AddScoped<ICsvImportService, CsvImportService>();

var app = builder.Build();

// Application automatique des migrations au démarrage
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur de migration : {ex.Message}");
    }
}

// Configuration du pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();