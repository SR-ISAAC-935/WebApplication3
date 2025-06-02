using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;
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

            try
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);

                var query = await (
                     from venta in _context.SalesClientResumes
                     join consumidor in _context.Consumidores
                         on venta.IdUsuario equals consumidor.IdConsumidor
                     where venta.FechaVenta == hoy
                     select new SalesClientResumeDTO
                     {
                        IdSales= venta.IdSales,
                        NombreCOnsumidor=consumidor.NombreConsumidor,
                        Total= venta.Total
                        

                     }
                ).ToListAsync();



                //var query = await _context.SalesClientResumes
                //    .Where(f => f.FechaVenta == hoy)
                //    .ToListAsync();


                if (query.Count == 0)
                {
                    TempData["MensajeError"] = "No hay ventas por el momento";
                    return RedirectToAction("ErrorAuth", "Home");
                }

                return View(query);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ocurrió un error al cargar las ventas del día.";
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
