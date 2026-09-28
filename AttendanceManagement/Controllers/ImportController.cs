using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AttendanceManagement.Services.Interfaces;

namespace AttendanceManagement.Controllers
{
    public class ImportController : Controller
    {
        private readonly ICsvImportService _csvImportService;

        // ETO NO NISY ERREUR: Tokony ICsvImportService fa tsy CsvImportService
        public ImportController(ICsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        // GET: /Import
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Import/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Veuillez sélectionner un fichier CSV.";
                return RedirectToAction("Index");
            }

            try
            {
                var success = await _csvImportService.ImportCsvAsync(file);
                if (success)
                {
                    TempData["SuccessMessage"] = "Le fichier a été importé avec succès !";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Échec de l'importation du fichier. Veuillez vérifier le format des données.";
                }
            }
            catch (Exception ex)
            {
                string detail = ex.InnerException != null ? " | " + ex.InnerException.Message : "";
                TempData["ErrorMessage"] = "Erreur d'importation : " + ex.Message + detail;
            }

            return RedirectToAction("Index");
        }
    }
}