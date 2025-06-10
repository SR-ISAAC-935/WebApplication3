using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication3.Custom;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly LumitecContext _context;
        private readonly ILogger<ClsListaNegraMetodos> _logger;
        private readonly VentasMethods _ventasMethods;
        public VentasController(LumitecContext context, ILogger<ClsListaNegraMetodos> logger, VentasMethods methods1)
        {
            _context = context;
            _logger = logger;
            _ventasMethods = methods1;
        }
        public IActionResult Sales()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Supongamos que el nombre del usuario está almacenado en la propiedad Name del token
                ViewData["Username"] = User.Identity.Name;
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RealizarVenta([FromBody] CrearVentaRequest request)
        {
         
           

            if (request == null)
            {
                return StatusCode(500, new { mensaje = "No se envió nada" });
            }

            try
            {
                // Deserializa ConsumidoresJson
                var detalles = JsonConvert.DeserializeObject<List<VentasDTO>>(request.ConsumidoresJson);               
                // Lógica de negocio (simulada con await)
                await _ventasMethods.Vendido(detalles, request.DeudaTotal);

                return StatusCode(200, new { mensaje = "Venta realizada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error: {ex.Message}" });
            }
        }


    }

}
