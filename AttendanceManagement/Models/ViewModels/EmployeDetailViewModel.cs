using System;
using System.Collections.Generic;
using AttendanceManagement.Models;

namespace AttendanceManagement.Models.ViewModels
{
    public class EmployeDetailViewModel
    {
        public Employe Employe { get; set; } = null!;
        public DateTime DateFiltre { get; set; }
        public List<Pointage> Pointages { get; set; } = new List<Pointage>();
        public double TotalHeuresCalculated { get; set; }
        public List<string> Anomalies { get; set; } = new List<string>();
    }
}