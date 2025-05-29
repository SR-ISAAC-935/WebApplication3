using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using WebApplication3.Custom;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;
using Microsoft.EntityFrameworkCore;
namespace WebApplication3.Controllers
{
    [Authorize]
    public class DeudorController(ILogger<DeudorController> logger, LumitecContext lumitecContext, ClsListaNegraMetodos services) : Controller
    {
        public IActionResult CrearDeudor()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Supongamos que el nombre del usuario está almacenado en la propiedad Name del token
                ViewData["Username"] = User.Identity.Name;
            }
            return View();

        }
        public IActionResult AgregarDeuda()
        {

            return View();
        }
        public IActionResult GestionDeudas()
        {
            return View();
        }

        public IActionResult CancelarProductos()
        {
            return View();
        }
        [HttpGet]
        public IActionResult BuscarConsumidor(string term)
        {
            logger.LogInformation("Término de búsqueda: {Term}", term);

            var consumidores = lumitecContext.Consumidores
                .Where(c => c.NombreConsumidor.Contains(term))
                .Select(c => new { idConsumidor = c.IdConsumidor, nombreConsumidor = c.NombreConsumidor })
                .ToList();

            logger.LogDebug("Consumidores encont   rados: {Consumidores}", consumidores);

            return Json(consumidores);
        }
        [HttpGet]
        public IActionResult BuscarProductos(string term)
        {
            logger.LogInformation("Término de búsqueda: {Term}", term);

            var productos = lumitecContext.Products
            .Where(c => c.ProductName.Contains(term))
                .Select(c => new { idProducto = c.IdProduct, nombreProducto = c.ProductName, precioProducto = c.ProductPrices })
                .ToList();

            logger.LogDebug("Consumidores encont   rados: {Consumidores}", productos);

            return Json(productos);
        }

        [HttpPost]
        public async Task<IActionResult> Modificar([FromBody] CrearListaNegraRequest request)
        {
            Console.WriteLine(request);
            if (request == null || string.IsNullOrWhiteSpace(request.ConsumidoresJson))
            {
                return BadRequest(new { mensaje = "No se recibieron registros desde el cliente." });
            }

            var detalles = JsonConvert.DeserializeObject<List<DetalleListaNegraDTO>>(request.ConsumidoresJson);
            await services.ModificarListaNegraConDetalles(detalles, request.DeudaTotal);

            return Ok(new { mensaje = "Listo tengo tus datos bro" });
        }

        // Acción para manejar la creación de lista negra con detalles
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearListaNegraRequest request)
        {
            Console.WriteLine(request.ConsumidoresJson);
            if (request == null || string.IsNullOrWhiteSpace(request.ConsumidoresJson))
            {
                return BadRequest(new { mensaje = "No se recibieron registros desde el cliente." });
            }

            var consumidores = JsonConvert.DeserializeObject<List<ConsumidorListaNegraDTO>>(request.ConsumidoresJson);

            foreach (var con in consumidores)
            {
                Console.WriteLine($"uid{con.IdConsumidor}  {con.Productos.ToArray()}");
            }
            // Aplanar los detalles para procesarlos
            var detalles = consumidores
                .SelectMany(c => c.Productos.Select(p => new DetalleListaNegraDTO
                {
                    IdConsumidor = c.IdConsumidor,
                    IdProducto = p.IdProducto,
                    Cantidad = p.Cantidad,
                    Precio = p.Precio,
                    Deuda = request.DeudaTotal,
                }))
                .ToList();

            foreach (var det in detalles)
            {
                Console.WriteLine($"id{det.IdConsumidor} Producto ID: {det.IdProducto}, Cantidad: {det.Cantidad}, Precio: {det.Precio}");
            }

            await services.InsertarListaNegraConDetalles(detalles, request.DeudaTotal);

            return Ok(new { mensaje = "Lista negra creada con éxito." });
        }





        // Método para ejecutar el procedimiento almacenado
        private async Task InsertarListaNegraConDetalles(List<DetalleListaNegraDTO> detalles, decimal deudaTotal)
        {
            services.InsertarListaNegraConDetalles(detalles, deudaTotal);

        }
        // Ejemplo del método de lógica para obtener las deudas
        public List<DeudaDTO> ObtenerDeudasPorConsumidor(int idConsumidor)
        {
            return services.ObtenerDeudasPorConsumidor(idConsumidor);
        }

        [HttpPut]
        public IActionResult ActualizarDeuda(int idListado, double abono)
        {
            try
            {
                services.ActualizarDeuda(idListado, abono);
                return Ok(new { mensaje = "Abono realizado correctamente." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar deuda");
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Abonos(int idListado, decimal abonoo, int idUsuario)
        {
            try
            {
                if (idListado <= 0 || abonoo <= 0)
                {
                    return BadRequest(new
                    {
                        mensaje = "Los parámetros deben ser mayores a cero.",
                        datos = new { idListado, abonoo }
                    });
                }

                var resultado = services.InsertarAbonos(idListado, abonoo, idUsuario);

                return Ok(new
                {
                    mensaje = "Abono registrado correctamente.",
                    resultado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al registrar el abono.",
                    detalles = ex.Message,
                    datos = new { idListado, abonoo }
                });
            }
        }

   
        [HttpGet]
        public IActionResult DetalleAbonos(int idListado)
        {
            Console.WriteLine($"el id de listado es el siguente{idListado}");
            if (idListado <= 0)
            {

                logger.LogWarning("Intento de obtener detalles de deuda con un idListado no válido: {IdListado}", idListado);
                return BadRequest(new { mensaje = "el detalle no esta disponible o esta cancelado" });
            }
            try
            {
                var detalles = ObtenerDetallesAbonoDTO(idListado);
                if (detalles == null)
                {
                    return Ok(new { mensaje = $"no hay detalle que mostrar  {idListado}", datos = new List<Object>() });
                }
                foreach (var detalle in detalles)
                {
                    Console.WriteLine($" los detalles son {detalle.id} {detalle.name} {detalle.Abonado} {detalle.FechaAbono} {detalle.total}");
                }
                if (detalles.Count == 0)
                {
                    return Ok(new { mensaje = $"no hay detalle que mostrar {idListado}", datos = new List<Object>() });
                }
                return Ok(new { mensaje = "Datos cargados correctamente", datos = detalles });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener los detalles de la deuda para el idListado: {IdListado}", idListado);
                return StatusCode(500, new { mensaje = $"Hubo un error al obtener los detalles de la deuda. {idListado}" });
            }


        }
        [HttpGet]
        public IActionResult ObtenerDetallesDeDeuda(int idListado)
        {
            if (idListado <= 0)
            {
                logger.LogWarning("Intento de obtener detalles de deuda con un idListado no válido: {IdListado}", idListado);
                return BadRequest(new { mensaje = "El idListado proporcionado no es válido." });
            }

            try
            {
                // Llamar al servicio o a la base de datos para obtener los detalles de la deuda
                var detalles = ObtenerDetallesPorListado(idListado);
                if (detalles == null || !detalles.Any())
                {
                    logger.LogInformation("No se encontraron detalles para el idListado: {IdListado}", idListado);
                    return Ok(new List<object>()); // Devuelve una lista vacía
                }

                foreach (var detalle in detalles)
                {
                    logger.LogInformation("Producto: {Producto}, Cantidad: {Cantidad}, Precio: {Precio}, Total: {Total}",
                        detalle.Producto, detalle.Cantidad, detalle.Precio, detalle.Total);
                   
                }

                // Retornar los datos en formato JSON
                return Json(detalles);
            }
            catch (Exception ex)
            {
                // Manejo de errores
                logger.LogError(ex, "Error al obtener los detalles de la deuda para el idListado: {IdListado}", idListado);
                return StatusCode(500, new { mensaje = "Hubo un error al obtener los detalles de la deuda." });
            }
        }

        // Ejemplo del método de lógica para obtener los detalles
        public List<DetalleDeudaDTO> ObtenerDetallesPorListado(int idListado)
        {
            return services.ObtenerDetallesPorListado(idListado);

        }

        public List<AbonoDTO> ObtenerDetallesAbonoDTO(int idListado)
        {
            Console.WriteLine($" el id del listado en el metodo es de {idListado}");
            var detalles = services.ObtenerAbono(idListado);
            foreach (var detalle in detalles)
            {

                Console.WriteLine($" los detalles en el servicio  son {detalle.id} {detalle.name} {detalle.Abonado} {detalle.FechaAbono} {detalle.total}");
            }
         
            return services.ObtenerAbono(idListado);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
