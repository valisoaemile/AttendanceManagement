using AttendanceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
     

        public DbSet<Employe> Employes { get; set; }
        public DbSet<Pointage> Pointages { get; set; }
        public DbSet<Equipement> Equipements { get; set; }
        public DbSet<HistoriqueAction> HistoriqueActions { get; set; }


    }
}