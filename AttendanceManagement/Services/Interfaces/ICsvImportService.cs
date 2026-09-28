using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AttendanceManagement.Services.Interfaces
{
    public interface ICsvImportService
    {
        Task<bool> ImportCsvAsync(IFormFile file);
    }
}