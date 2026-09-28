using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Models
{
    public class HistoriqueAction
    {
        [Key]
        public int Id { get; set; }
        public int EmployeId { get; set; }
        public DateTime DatePointage { get; set; }
        public string TypeAction { get; set; } // "Modification" ou "Suppression"
        public string Description { get; set; }
        public DateTime DateAction { get; set; } = DateTime.Now;
    }
}