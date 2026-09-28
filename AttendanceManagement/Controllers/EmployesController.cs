using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Data;
using AttendanceManagement.Models;

namespace AttendanceManagement.Controllers
{
    public class EmployesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Employes
                .Include(e => e.Pointages)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e => e.Nom.Contains(searchString) || e.Matricule.Contains(searchString));
            }

            var employes = await query.ToListAsync();

            ViewBag.SearchString = searchString;
            return View(employes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employe = await _context.Employes
                .Include(e => e.Pointages)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employe == null)
            {
                return NotFound();
            }

            return View(employe);
        }
    }
}