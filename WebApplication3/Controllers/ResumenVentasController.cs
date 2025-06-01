using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication3.Models;
using YourNamespace.Controllers;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class ResumenVentasController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly LumitecContext _context;

        public ResumenVentasController(ILogger<ProductController> logger, LumitecContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> ResumenHoy()
        {

           
            if (User.IsInRole("Trabajador"))
            {
                TempData["MensajeError"] = "No está autorizado para acceder a esta página.";
                return RedirectToAction("ErrorAuth", "Home");
            }
            try {
                var Query =  _context.SalesClientResumes.Where(f => f.FechaVenta.Equals(DateTime.Today)).ToListAsync();

                if (!Query.Result.Equals(0))
                {
                    TempData["MensajeError"] = "No hay ventas por el momento, espera unos minutos u horas";
                    return RedirectToAction("ErrorAuth", "Home");
                }
                

                return View(Query);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ocurrió un error al cargar las ventas del dia.";
                Console.Error.WriteLine($"Error: {ex.Message}");
                return View("ResumenHoy", new List<SalesClientResume>());
            }
        }
        public IActionResult Vendido()
        {
            if (User.IsInRole("Trabajador"))
            {
                TempData["MensajeError"] = "No está autorizado para acceder a esta página.";
                return RedirectToAction("ErrorAuth", "Home");
            }
            return View();
        }
    }
}
