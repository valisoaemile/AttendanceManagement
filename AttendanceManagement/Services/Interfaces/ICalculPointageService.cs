using System;
using System.Collections.Generic;
using AttendanceManagement.Models;

namespace AttendanceManagement.Services.Interfaces
{
    public interface ICalculPointageService
    {
        double CalculerTotalHeures(IEnumerable<Pointage> pointages);
        List<string> DetecterAnomalies(IEnumerable<Pointage> pointages);
    }
}