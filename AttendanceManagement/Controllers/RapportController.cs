using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;

namespace AttendanceManagement.Controllers
{
    public class RapportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RapportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Page de consultation du rapport sur le site
        public async Task<IActionResult> Index(DateTime? date)
        {
            DateTime targetDate = date.HasValue ? date.Value.Date : DateTime.Today;

            // Fitiliana: raha tsy nametraka date ny mpampiasa ary tsy misy pointage androany dia raiso ny date farany nisy pointage
            if (!date.HasValue && !await _context.Pointages.AnyAsync(p => p.DatePointage.Date == targetDate))
            {
                var lastPointage = await _context.Pointages.OrderByDescending(p => p.DatePointage).FirstOrDefaultAsync();
                if (lastPointage != null)
                {
                    targetDate = lastPointage.DatePointage.Date;
                }
            }

            var pointages = await _context.Pointages
                .AsNoTracking()
                .Include(p => p.Employe)
                .Where(p => p.DatePointage.Date == targetDate)
                .OrderByDescending(p => p.HeurePointage)
                .ToListAsync();

            ViewBag.SelectedDate = targetDate.ToString("yyyy-MM-dd");
            ViewBag.TotalEntries = pointages.Count;

            return View(pointages);
        }

        // 2. Exportation CSV IDENTIQUE À 100% À L'ORIGINAL BIOSTAR 2
        public async Task<IActionResult> ExporterCsv(DateTime? date)
        {
            DateTime targetDate = date.HasValue ? date.Value.Date : DateTime.Today;

            if (!date.HasValue && !await _context.Pointages.AnyAsync(p => p.DatePointage.Date == targetDate))
            {
                var lastPointage = await _context.Pointages.OrderByDescending(p => p.DatePointage).FirstOrDefaultAsync();
                if (lastPointage != null)
                {
                    targetDate = lastPointage.DatePointage.Date;
                }
            }

            // Récupération des pointages (raha nisy modification na suppression dia ireo vaovao ihany no mivoaka eto)
            var pointages = await _context.Pointages
                .AsNoTracking()
                .Include(p => p.Employe)
                .Where(p => p.DatePointage.Date == targetDate)
                .OrderByDescending(p => p.HeurePointage)
                .ToListAsync();

            var builder = new StringBuilder();

            // 1. En-tête officiel BioStar 2 (Séparateur virgule)
            builder.AppendLine("Date,Porte,ID de l'Équipement,Equipement,Group Utilisateur,Utilisateur,Evènement");

            foreach (var p in pointages)
            {
                // 2. Format d'Heure tsara ho an'ny TimeSpan sy Date yyyy-MM-dd HH:mm:ss
                string heureStr = p.HeurePointage.ToString(@"hh\:mm\:ss");
                string dateTimeStr = $"{p.DatePointage:yyyy-MM-dd} {heureStr}";

                string porte = string.IsNullOrEmpty(p.Porte) ? "" : p.Porte;
                string idEquip = string.IsNullOrEmpty(p.IdEquipement) ? "" : p.IdEquipement;
                string equipement = string.IsNullOrEmpty(p.Equipement) ? "" : p.Equipement;

                // 3. Groupe d'utilisateur mi-aligner amin'ny BioStar 2
                string groupe = "Tous les Utilisateurs";

                string utilisateur = "";
                if (p.Employe != null)
                {
                    utilisateur = $"{p.Employe.Matricule}({p.Employe.Nom})";
                }

                string evenement = string.IsNullOrEmpty(p.Evenement)
                    ? "Succès de l'Authentification1:N\\n (Fingerprint)"
                    : p.Evenement;

                // 4. Assemblage exact amin'ny virgule (,)
                builder.AppendLine($"{dateTimeStr},{porte},{idEquip},{equipement},{groupe},{utilisateur},{evenement}");
            }

            // 5. Génération du nom de fichier BioStar 2 exact (ex: Report_20260928T082421.csv)
            string nowTimestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string fileName = $"Report_{nowTimestamp}.csv";

            // 6. Encodage UTF-8 sans BOM (recommandé par BioStar)
            var encoding = new UTF8Encoding(false);
            byte[] fileBytes = encoding.GetBytes(builder.ToString());

            return File(fileBytes, "text/csv", fileName);
        }
    }
}