using System;

namespace AttendanceManagement.Models
{
    public class Anomalie
    {
        public int Id { get; set; }
        public int EmployeId { get; set; }
        public string Matricule { get; set; } = string.Empty;
        public string NomEmploye { get; set; } = string.Empty;
        public DateTime DatePointage { get; set; }
        public double TotalHeures { get; set; }
        public string TypeAnomalie { get; set; } = "HEURE_INSUFFISANTE";
        public string Message { get; set; } = "Durée inférieure à 9h";
    }
}