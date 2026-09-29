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

// Configuration du DbContext : Npgsql sur Render / SQL Server ou Npgsql selon le provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // En local : Utilise SQL Server (ou la base locale configurée dans appsettings.Development.json)
        options.UseSqlServer(connectionString);
    }
    else
    {
        // Sur Render : Utilise PostgreSQL
        options.UseNpgsql(connectionString);
    }
});

// Injection de dépendances
builder.Services.AddScoped<ICsvImportService, CsvImportService>();

var app = builder.Build();

// Auto-apply Database Migrations uniquement en Production (sur Render)
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
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