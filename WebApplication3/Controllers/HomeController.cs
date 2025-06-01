using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LumitecContext _context;
        public HomeController(ILogger<HomeController> logger , LumitecContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Productos() { return View(); }
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Supongamos que el nombre del usuario está almacenado en la propiedad Name del token
                ViewData["Username"] = User.Identity.Name;
                ViewData["Rol"] = User.FindFirst(ClaimTypes.Role)?.Value;
            }

            return View();
        }
        public IActionResult ErrorAuth()
        {
            ViewBag.MensajeError = TempData["MensajeError"];
            return View();
        }


        public async Task<IActionResult> Consumidor()
        {
            if (User.IsInRole("Trabajador"))
            {
                TempData["MensajeError"] = "No está autorizado para acceder a esta página.";
                return RedirectToAction("ErrorAuth", "Home");
            }

            try
            {
                var consum = _context.Consumidores.AsQueryable();
                var comp = await consum.ToListAsync(); // <- aquí está la corrección
                return View(comp);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ocurrió un error al cargar los consumidores.";
                Console.Error.WriteLine($"Error: {ex.Message}");
                return View("ErrorAuth", new List<Consumidores>());
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
