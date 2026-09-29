using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Services.Interfaces;
using AttendanceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Récupération de la chaîne de connexion
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Configuration du DbContext: Tsindriana ampiasaina ny Npgsql rehefa misy Host/PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString != null && connectionString.Contains("Host="))
    {
        // Rohy PostgreSQL (Render na Cloud)
        options.UseNpgsql(connectionString);
    }
    else
    {
        // Rohy SQL Server (LocalDB)
        options.UseSqlServer(connectionString);
    }
});

// Injection de dépendances
builder.Services.AddScoped<ICsvImportService, CsvImportService>();

var app = builder.Build();

// Auto-apply Database Migrations rehefa PostgreSQL no ampiasaina
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsPostgreSQL())
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
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