using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class ConsumidorController : Controller
    {
        private readonly ILogger<ConsumidorController> _logger;
        LumitecContext _context;
        public ConsumidorController(ILogger<ConsumidorController> logger, LumitecContext lumitecContext)
        {
            _logger = logger;
            _context = lumitecContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return (IViewComponentResult)View("/Views/Partials/_ConsumidorPartials.cshtml");
        }
        public IActionResult CrearConsumidor()
        {
           
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Supongamos que el nombre del usuario está almacenado en la propiedad Name del token
                ViewData["Username"] = User.Identity.Name;
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var role = _context.ConsuElectricistas.Select(c => new ConsuElectricista
            {
                Id = c.Id,
                Descripcion = c.Descripcion
            }).ToList();

            return Ok(role);
        }
        [HttpPost]
        public async Task<IActionResult> CrearConsumidores([FromBody] List<Consumidore> consumidores)
        {
            if (consumidores != null && consumidores.Any())
            {
                try
                {
                    //Convertir la lista de consumidores a JSON
                   var consumidoresJson = JsonConvert.SerializeObject(consumidores);

                    // Ejecutar el procedimiento almacenado para insertar los consumidores
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC CrearConsumidor @ConsumidoresJson = {0}", consumidoresJson);
                    foreach (var lot in consumidores)
                    {
                        Console.WriteLine(lot.IdRole +"\t"+ lot.NombreConsumidor);
                    }
                    var response = new
                    {
                        Mensaje = "Consumidores guardados exitosamente.",
                        Datos = consumidores
                    };

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    // Manejo de errores
                    TempData["Mensaje"] = "Error al guardar los consumidores: " + ex.Message;
                    return BadRequest();
                }
            }
            else
            {
                TempData["Mensaje"] = "No se han enviado consumidores.";
                return BadRequest();
            }
        }
    }
}
