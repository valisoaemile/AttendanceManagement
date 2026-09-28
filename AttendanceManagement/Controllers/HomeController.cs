using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;

namespace AttendanceManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TABLEAU DE BORD (INDEX)
        public async Task<IActionResult> Index(DateTime? dateFilter)
        {
            DateTime dateCible;

            if (dateFilter.HasValue)
            {
                dateCible = dateFilter.Value.Date;
            }
            else
            {
                var dernierPointageInsere = await _context.Pointages
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                if (dernierPointageInsere == null)
                {
                    ViewBag.Anomalies = new List<Anomalie>();
                    ViewBag.DateAffichee = DateTime.Today;
                    return View(new List<Pointage>());
                }

                dateCible = dernierPointageInsere.DatePointage.Date;
            }

            var rawPointages = await _context.Pointages
                .AsNoTracking()
                .Include(p => p.Employe)
                .Where(p => p.DatePointage.Date == dateCible)
                .OrderByDescending(p => p.HeurePointage)
                .ToListAsync();

            var listAnomalies = new List<Anomalie>();

            var pointagesParEmploye = rawPointages
                .Where(p => p.EmployeId.HasValue
                         && p.EmployeId.Value > 0
                         && p.Employe != null
                         && p.Employe.Matricule != "INCONNU")
                .GroupBy(p => p.EmployeId.Value);

            foreach (var group in pointagesParEmploye)
            {
                var emp = group.First().Employe;
                var listOrderedDesc = group.OrderByDescending(p => p.HeurePointage).ToList();

                if (!listOrderedDesc.Any()) continue;

                // FITILIANA DOUBLON
                bool hasDoublon = false;
                string equipementDoublon = "";

                for (int i = 0; i < listOrderedDesc.Count - 1; i++)
                {
                    var curEquip = listOrderedDesc[i].Equipement?.Trim() ?? "";
                    var nextEquip = listOrderedDesc[i + 1].Equipement?.Trim() ?? "";

                    if (!string.IsNullOrEmpty(curEquip) && curEquip.Equals(nextEquip, StringComparison.OrdinalIgnoreCase))
                    {
                        hasDoublon = true;
                        equipementDoublon = curEquip;
                        break;
                    }
                }

                // KAJY TOTAL HEURES
                TimeSpan reposDebut = new TimeSpan(12, 29, 59);
                TimeSpan reposFin = new TimeSpan(13, 30, 0);
                TimeSpan reposAjuste = new TimeSpan(13, 30, 0);

                TimeSpan totalHeures = TimeSpan.Zero;

                for (int i = 0; i < listOrderedDesc.Count - 1; i++)
                {
                    var pUpper = listOrderedDesc[i];     // Sortie R2
                    var pLower = listOrderedDesc[i + 1]; // Entrée P2

                    var equipUpper = pUpper.Equipement ?? "";
                    var equipLower = pLower.Equipement ?? "";

                    if (equipUpper.Contains("R2", StringComparison.OrdinalIgnoreCase) &&
                        equipLower.Contains("P2", StringComparison.OrdinalIgnoreCase))
                    {
                        TimeSpan heureUpper = pUpper.HeurePointage;
                        TimeSpan heureLower = pLower.HeurePointage;

                        if (heureUpper >= reposDebut && heureUpper < reposFin) heureUpper = reposAjuste;
                        if (heureLower >= reposDebut && heureLower < reposFin) heureLower = reposAjuste;

                        TimeSpan dureeSession = heureUpper - heureLower;

                        if (dureeSession > TimeSpan.Zero)
                        {
                            totalHeures += dureeSession;
                        }
                    }
                }

                // FITILIANA: Durée globale inférieure à 9h
                bool hasHeureInsuffisante = totalHeures < new TimeSpan(9, 0, 0);

                if (hasDoublon && hasHeureInsuffisante)
                {
                    listAnomalies.Add(new Anomalie
                    {
                        EmployeId = group.Key,
                        Matricule = emp?.Matricule ?? "",
                        NomEmploye = emp?.Nom ?? "",
                        DatePointage = dateCible,
                        TotalHeures = Math.Round(totalHeures.TotalHours, 2),
                        TypeAnomalie = "DOUBLE_EQUIPEMENT_ET_HEURE_INSUFFISANTE",
                        Message = $"Double équipement ({equipementDoublon}) ET durée globale inférieure à 9h ({totalHeures:hh\\:mm\\:ss})"
                    });
                }
                else if (hasDoublon)
                {
                    listAnomalies.Add(new Anomalie
                    {
                        EmployeId = group.Key,
                        Matricule = emp?.Matricule ?? "",
                        NomEmploye = emp?.Nom ?? "",
                        DatePointage = dateCible,
                        TotalHeures = Math.Round(totalHeures.TotalHours, 2),
                        TypeAnomalie = "DOUBLON_EQUIPEMENT",
                        Message = $"Double équipement détecté sur {equipementDoublon}"
                    });
                }
                else if (hasHeureInsuffisante)
                {
                    listAnomalies.Add(new Anomalie
                    {
                        EmployeId = group.Key,
                        Matricule = emp?.Matricule ?? "",
                        NomEmploye = emp?.Nom ?? "",
                        DatePointage = dateCible,
                        TotalHeures = Math.Round(totalHeures.TotalHours, 2),
                        TypeAnomalie = "HEURE_INSUFFISANTE",
                        Message = $"Durée globale inférieure à 9h ({totalHeures:hh\\:mm\\:ss})"
                    });
                }
            }

            ViewBag.DateAffichee = dateCible;
            ViewBag.Anomalies = listAnomalies;
            return View(rawPointages);
        }

        // 2. VOIR ET CORRIGER LES POINTAGES D'UN EMPLOYÉ
        public async Task<IActionResult> VoirPointage(int employeId, DateTime? date)
        {
            var employe = await _context.Employes.FindAsync(employeId);

            if (!date.HasValue)
            {
                date = await _context.Pointages
                    .Where(p => p.EmployeId.HasValue && p.EmployeId.Value == employeId)
                    .OrderByDescending(p => p.DatePointage)
                    .Select(p => p.DatePointage.Date)
                    .FirstOrDefaultAsync();
            }

            if (!date.HasValue || date.Value == default)
            {
                date = DateTime.Today;
            }

            var rawPointages = await _context.Pointages
                .AsNoTracking()
                .Where(p => p.EmployeId.HasValue && p.EmployeId.Value == employeId && p.DatePointage.Date == date.Value.Date)
                .OrderByDescending(p => p.HeurePointage)
                .ToListAsync();

            ViewBag.Employe = employe;
            ViewBag.DatePointage = date;

            return View(rawPointages);
        }

        // 3. PAGE DES EMPLOYÉS
        public async Task<IActionResult> Employes(string search)
        {
            var query = _context.Employes.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(e => e.Nom.ToLower().Contains(search) || e.Matricule.ToLower().Contains(search));
            }

            var listeEmployes = await query.OrderBy(e => e.Matricule).ToListAsync();

            ViewBag.Search = search;
            return View(listeEmployes);
        }

        // 4. PAGE RAPPORTS (Aligné sur l'ordre BioStar 2 : trié par heure décroissante)
        public async Task<IActionResult> Rapports(DateTime? dateFilter)
        {
            DateTime dateCible;

            if (dateFilter.HasValue)
            {
                dateCible = dateFilter.Value.Date;
            }
            else
            {
                var dernierPointage = await _context.Pointages
                    .Where(p => p.EmployeId.HasValue && p.Employe.Matricule != "INCONNU")
                    .OrderByDescending(p => p.DatePointage)
                    .FirstOrDefaultAsync();

                if (dernierPointage == null)
                {
                    ViewBag.DateAffichee = DateTime.Today;
                    return View(new List<Pointage>());
                }

                dateCible = dernierPointage.DatePointage.Date;
            }

            var pointages = await _context.Pointages
                .AsNoTracking()
                .Include(p => p.Employe)
                .Where(p => p.DatePointage.Date == dateCible
                         && p.EmployeId.HasValue
                         && p.Employe.Matricule != "INCONNU")
                .OrderByDescending(p => p.HeurePointage) // Tri identique à l'affichage BioStar 2
                .ToListAsync();

            ViewBag.DateAffichee = dateCible;
            return View(pointages);
        }
        // 5. EXPORTER LE FICHIER CSV (Identique à 100% à l'export original BioStar 2)
        [HttpGet]
        public async Task<IActionResult> ExporterCsv(DateTime? dateFilter)
        {
            DateTime dateCible;

            if (dateFilter.HasValue)
            {
                dateCible = dateFilter.Value.Date;
            }
            else
            {
                var dernierPointage = await _context.Pointages
                    .Where(p => p.EmployeId.HasValue && p.Employe.Matricule != "INCONNU")
                    .OrderByDescending(p => p.DatePointage)
                    .FirstOrDefaultAsync();

                dateCible = dernierPointage != null ? dernierPointage.DatePointage.Date : DateTime.Today;
            }

            // Récupération triée par heure décroissante (comme l'original)
            var pointages = await _context.Pointages
                .AsNoTracking()
                .Include(p => p.Employe)
                .Where(p => p.DatePointage.Date == dateCible
                         && p.EmployeId.HasValue
                         && p.Employe.Matricule != "INCONNU")
                .OrderByDescending(p => p.HeurePointage)
                .ToListAsync();

            var builder = new StringBuilder();

            // En-tête exact de BioStar 2 (Séparateur virgule)
            builder.AppendLine("Date,Porte,ID de l'Équipement,Equipement,Group Utilisateur,Utilisateur,Evènement");

            foreach (var item in pointages)
            {
                // Format d'heure correct pour TimeSpan / DateTime
                string heureStr = item.HeurePointage.ToString(@"hh\:mm\:ss");
                string dateFormatted = $"{item.DatePointage:yyyy-MM-dd} {heureStr}";

                string porte = string.IsNullOrEmpty(item.Porte) ? "" : item.Porte;
                string idEquip = string.IsNullOrEmpty(item.IdEquipement) ? "" : item.IdEquipement;
                string equipement = string.IsNullOrEmpty(item.Equipement) ? "" : item.Equipement;
                string grp = "Tous les Utilisateurs";
                string util = item.Employe != null ? $"{item.Employe.Matricule}({item.Employe.Nom})" : "";

                string evt = string.IsNullOrWhiteSpace(item.Evenement)
                    ? "Succès de l'Authentification1:N\\n (Fingerprint)"
                    : item.Evenement;

                // Construction avec virgule (,)
                builder.AppendLine($"{dateFormatted},{porte},{idEquip},{equipement},{grp},{util},{evt}");
            }

            // Format de nom exact BioStar 2 (ex: Report_20260928T082421.csv)
            string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string fileName = $"Report_{timestamp}.csv";

            // Encodage UTF-8 sans BOM
            var encoding = new UTF8Encoding(false);
            return File(encoding.GetBytes(builder.ToString()), "text/csv", fileName);
        }

        

        // 6. MODIFIER ET SUPPRIMER
        [HttpPost]
        public async Task<IActionResult> ModifierHeure(int id, int employeId, string date, string nouvelleHeure)
        {
            var pointage = await _context.Pointages.FindAsync(id);
            if (pointage != null && TimeSpan.TryParse(nouvelleHeure, out TimeSpan heureParsed))
            {
                var ancienneHeure = pointage.HeurePointage;

                DateTime dateCible = pointage.DatePointage.Date;
                var listPointagesDate = await _context.Pointages
                    .AsNoTracking()
                    .Include(p => p.Employe)
                    .Where(p => p.DatePointage.Date == dateCible
                             && p.EmployeId.HasValue
                             && p.Employe.Matricule != "INCONNU")
                    .OrderByDescending(p => p.HeurePointage)
                    .ToListAsync();

                int numLigneCsv = listPointagesDate.FindIndex(p => p.Id == id) + 2;

                pointage.HeurePointage = heureParsed;
                await _context.SaveChangesAsync();

                TempData["ActionMessage"] = $"L'heure du pointage sur {pointage.Equipement} (Ligne CSV n°{numLigneCsv}) a été modifiée de {ancienneHeure:hh\\:mm\\:ss} à {heureParsed:hh\\:mm\\:ss}.";
                TempData["ActionType"] = "warning";
            }

            return RedirectToAction("VoirPointage", new { employeId = employeId, date = date });
        }

        [HttpPost]
        public async Task<IActionResult> SupprimerPointage(int id, int employeId, string date)
        {
            var pointage = await _context.Pointages.FindAsync(id);
            if (pointage != null)
            {
                DateTime dateCible = pointage.DatePointage.Date;
                var listPointagesDate = await _context.Pointages
                    .AsNoTracking()
                    .Include(p => p.Employe)
                    .Where(p => p.DatePointage.Date == dateCible
                             && p.EmployeId.HasValue
                             && p.Employe.Matricule != "INCONNU")
                    .OrderByDescending(p => p.HeurePointage)
                    .ToListAsync();

                int numLigneCsv = listPointagesDate.FindIndex(p => p.Id == id) + 2;

                string info = $"Le pointage de {pointage.HeurePointage:hh\\:mm\\:ss} sur {pointage.Equipement} (Ligne CSV n°{numLigneCsv}) a été supprimé.";

                _context.Pointages.Remove(pointage);
                await _context.SaveChangesAsync();

                TempData["ActionMessage"] = info;
                TempData["ActionType"] = "danger";
            }

            return RedirectToAction("VoirPointage", new { employeId = employeId, date = date });
        }
        // 7. SUPPRIMER UN EMPLOYÉ ET TOUS SES POINTAGES
        [HttpPost]
        public async Task<IActionResult> SupprimerEmploye(int id)
        {
            var employe = await _context.Employes
                .Include(e => e.Pointages) // Inclure les pointages pour les supprimer en cascade
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employe != null)
            {
                // 1. Supprimer d'abord les pointages de cet employé
                if (employe.Pointages != null && employe.Pointages.Any())
                {
                    _context.Pointages.RemoveRange(employe.Pointages);
                }

                // 2. Supprimer l'employé
                _context.Employes.Remove(employe);

                await _context.SaveChangesAsync();

                TempData["ActionMessage"] = $"L'employé {employe.Matricule} ({employe.Nom}) et tous ses pointages ont été supprimés avec succès.";
                TempData["ActionType"] = "success";
            }

            return RedirectToAction("Employes");
        }
    }
}