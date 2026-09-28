using System;

namespace AttendanceManagement.Models
{
    public class Pointage
    {
        public int Id { get; set; }
        public DateTime DatePointage { get; set; }
        public TimeSpan HeurePointage { get; set; }

        public string? Porte { get; set; }
        public string? IdEquipement { get; set; }

        public string Equipement { get; set; } = string.Empty;
        public string Evenement { get; set; } = string.Empty;

        public TimeSpan? HeureModifiee { get; set; }
        public string? Type { get; set; }

        // Mettre int? au lieu de int pour autoriser NULL
        public int? EmployeId { get; set; }
        public Employe? Employe { get; set; }
    }
}