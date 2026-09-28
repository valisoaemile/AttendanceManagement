using System;
using System.Collections.Generic;
using System.Linq;
using AttendanceManagement.Models;
using AttendanceManagement.Services.Interfaces;

namespace AttendanceManagement.Services
{
    public class CalculPointageService : ICalculPointageService
    {
        public double CalculerTotalHeures(IEnumerable<Pointage> pointages)
        {
            if (pointages == null || !pointages.Any()) return 0;

            
            var pointagesValides = pointages
                .Where(p => p.Employe != null
                         && p.Employe.Matricule != "INCONNU"
                         && (string.IsNullOrEmpty(p.Evenement) || p.Evenement.Contains("Succès")))
                .OrderBy(p => p.HeureModifiee.HasValue ? p.HeureModifiee.Value : p.HeurePointage)
                .ToList();

            double totalMinutes = 0;

            //  Appariement Entrée / Sortie
            for (int i = 0; i < pointagesValides.Count - 1; i++)
            {
                var pActuel = pointagesValides[i];
                var pSuivant = pointagesValides[i + 1];

                if (pActuel.Type == "Entree" && pSuivant.Type == "Sortie")
                {
                    TimeSpan heure1 = pActuel.HeureModifiee ?? pActuel.HeurePointage;
                    TimeSpan heure2 = pSuivant.HeureModifiee ?? pSuivant.HeurePointage;

                    var duree = heure2 - heure1;
                    if (duree.TotalMinutes > 0)
                    {
                        totalMinutes += duree.TotalMinutes;
                    }
                    i++; 
                }
            }

            return Math.Round(totalMinutes / 60.0, 2);
        }

        public List<string> DetecterAnomalies(IEnumerable<Pointage> pointages)
        {
            var anomalies = new List<string>();
            if (pointages == null || !pointages.Any())
            {
                return anomalies;
            }

            
            var pointagesValides = pointages
                .Where(p => p.Employe != null
                         && p.Employe.Matricule != "INCONNU"
                         && (string.IsNullOrEmpty(p.Evenement) || p.Evenement.Contains("Succès")))
                .ToList();

            if (!pointagesValides.Any()) return anomalies;

            var entrees = pointagesValides.Count(p => p.Type == "Entree");
            var sorties = pointagesValides.Count(p => p.Type == "Sortie");

            if (entrees != sorties)
            {
                anomalies.Add($"Incohérence : Nombre d'entrées ({entrees}) ne correspond pas au nombre de sorties ({sorties}).");
            }

            return anomalies;
        }
    }
}