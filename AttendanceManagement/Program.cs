using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Services.Interfaces;
using AttendanceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// 1. Alaivo avy amin'ny Render environment variable na appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Jereo raha misy ilay connectionString na tsia
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Tsy hita ny Connection String 'DefaultConnection'. Jereo ny Environment Variables ao amin'ny Render.");
}

// 3. Amboary tsara ny DbContext miaraka amin'ny PostgreSQL (Npgsql)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injection de dépendances
builder.Services.AddScoped<ICsvImportService, CsvImportService>();

var app = builder.Build();

// Application automatique des migrations au démarrage
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
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