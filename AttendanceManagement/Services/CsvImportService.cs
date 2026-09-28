using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;
using AttendanceManagement.Services.Interfaces;

namespace AttendanceManagement.Services
{
    public class CsvImportService : ICsvImportService
    {
        private readonly ApplicationDbContext _context;

        public CsvImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            // 1. Fafana ny pointages taloha rehetra
            _context.Pointages.RemoveRange(_context.Pointages);
            await _context.SaveChangesAsync();

            using (var stream = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                string? line;
                bool isHeader = true;

                while ((line = await stream.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (isHeader)
                    {
                        isHeader = false;
                        continue;
                    }

                    char separator = line.Contains(';') ? ';' : (line.Contains('\t') ? '\t' : ',');
                    var cols = line.Split(separator);

                    if (cols.Length >= 7)
                    {
                        string dateRaw = cols[0].Trim('"', ' ');
                        string porteRaw = cols[1].Trim('"', ' ');
                        string idEquipRaw = cols[2].Trim('"', ' ');
                        string equipementRaw = cols[3].Trim('"', ' ');
                        string groupeRaw = cols[4].Trim('"', ' ');
                        string utilisateurRaw = cols[5].Trim('"', ' ');
                        string evenementRaw = cols[6].Trim('"', ' ');

                        // -------------------------------------------------------------------------
                        // 2. FILTRE SAKANA : Sakana tanteraka ny événements "Mise à jour / Update"
                        // -------------------------------------------------------------------------
                        if (!string.IsNullOrEmpty(evenementRaw))
                        {
                            string evt = evenementRaw.ToLower();

                            // Raha misy teny hoe "mise", "jour", "update", "mis a jour" dia AISORANA (skip)
                            if (evt.Contains("mise") || evt.Contains("jour") || evt.Contains("update") || evt.Contains("modification"))
                            {
                                continue;
                            }
                        }

                        if (DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fullDateTime) ||
                            DateTime.TryParse(dateRaw, out fullDateTime))
                        {
                            int? employeId = null;

                            if (!string.IsNullOrEmpty(utilisateurRaw) && utilisateurRaw.Contains("("))
                            {
                                var parts = utilisateurRaw.Split('(');
                                string matricule = parts[0].Trim();
                                string nom = parts[1].Replace(")", "").Trim();

                                var emp = await _context.Employes.FirstOrDefaultAsync(e => e.Matricule == matricule);
                                if (emp == null)
                                {
                                    emp = new Employe { Matricule = matricule, Nom = nom };
                                    _context.Employes.Add(emp);
                                    await _context.SaveChangesAsync();
                                }
                                employeId = emp.Id;
                            }

                            // 3. SAKANA NY DOUBLON EXACT (Mpiasa mitovy + Date mitovy + Heure mitovy)
                            if (employeId.HasValue)
                            {
                                bool dejaExiste = await _context.Pointages.AnyAsync(p =>
                                    p.EmployeId == employeId.Value &&
                                    p.DatePointage == fullDateTime.Date &&
                                    p.HeurePointage == fullDateTime.TimeOfDay);

                                if (dejaExiste)
                                {
                                    continue;
                                }
                            }

                            var pointage = new Pointage
                            {
                                DatePointage = fullDateTime.Date,
                                HeurePointage = fullDateTime.TimeOfDay,
                                Porte = porteRaw,
                                IdEquipement = idEquipRaw,
                                Equipement = equipementRaw,
                                Evenement = evenementRaw,
                                EmployeId = employeId
                            };

                            _context.Pointages.Add(pointage);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}