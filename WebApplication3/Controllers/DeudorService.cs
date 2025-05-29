using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeudorService : ControllerBase
    {
        private readonly LumitecContext _context;
        private readonly DeudorService _dedudasService;
        public DeudorService(LumitecContext lumitecContext, DeudorService deudorService)
        {
            _context = lumitecContext;
            _dedudasService = deudorService;
        }
        [HttpPost("ObtenerDeudas")]
        public async Task<IActionResult> ObtenerDeudas([FromBody] int idConsumidor)
        {
            if (idConsumidor <= 0)
            {
                return BadRequest(new { mensaje = "IdConsumidor no válido." });
            }

            try
            {
                var deudas = await _dedudasService.ObtenerDeudas(idConsumidor);

                if (deudas == null)
                {
                    return Ok(new { mensaje = "No se encontraron deudas para este consumidor." });
                }

                return Ok(deudas); // Devuelve la lista de deudas en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al procesar la solicitud.", error = ex.Message });
            }
        }

        [HttpPost("ObtenerDetallesDeDeuda")]
        public async Task<IActionResult> ObtenerDetallesDeDeuda([FromBody] int idListado)
        {
            if (idListado <= 0)
            {
                return BadRequest(new { mensaje = "IdListado no válido." });
            }

            try
            {
                var detallesDeuda = await _dedudasService.ObtenerDetallesDeDeuda(idListado);

                if (detallesDeuda == null)
                {
                    return Ok(new { mensaje = "No se encontraron detalles para esta deuda." });
                }

                return Ok(detallesDeuda); // Devuelve los detalles de la deuda en formato JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al procesar la solicitud.", error = ex.Message });
            }
        }
    }
}
