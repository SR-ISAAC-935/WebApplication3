using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace YourNamespace.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly LumitecContext _context;

        public ProductController(ILogger<ProductController> logger, LumitecContext context)
        {
            _logger = logger;
            _context = context;
        }
          public async Task<IActionResult> Inventario(string filtro)
          {
              try
              {
                  // Consulta inicial de productos
                  var query = _context.Products.AsQueryable();

                // Validar si hay productos en la base de datos
                if(!query.Any())
                  {
                      ViewBag.Message = "No hay datos en el inventario";
                      return View("Inventario", new List<Product>());
                  }

                  // Configurar filtros en ViewData
                  ViewData["FiltroNombre"] = string.IsNullOrEmpty(filtro) ? "NombreDescendente" : "";
                  ViewData["FiltroPrecio"] = string.IsNullOrEmpty(filtro)  ? "PrecioDescendente" : "";
                  ViewData["FiltroStock"] = string.IsNullOrEmpty(filtro)  ? "StockDescendente" : "";
                  ViewData["FiltroProveedor"] = string.IsNullOrEmpty(filtro)  ? "ProveedorDescendente" : "";

                  // Aplicar ordenamiento según el filtro
                  switch (filtro)
                  {
                      case "NombreDescendente":
                          query = query.OrderByDescending(p => p.ProductName);
                          break;
                      case "PrecioDescendente":
                          query = query.OrderByDescending(p => p.ProductPrices);
                          break;
                      case "StockDescendente":
                          query = query.OrderByDescending(p => p.ProductStock);
                          break;
                      case "ProveedorDescendente":
                          query = query.OrderByDescending(p => p.ProductProvider);
                          break;
                      default:
                          query = query.OrderBy(p => p.ProductName); // Orden predeterminado
                          break;
                  }

                  // Ejecutar la consulta y devolver los resultados
                  var productos = await query.ToListAsync();
                  return View( productos);
              }
              catch (Exception ex)
              {
                  // Manejar errores y pasar un mensaje a la vista
                  ViewBag.Message = "Ocurrió un error al cargar el inventario.";
                  Console.Error.WriteLine($"Error: {ex.Message}");
                  return View("Inventario", new List<Product>());
              }

          }
         public async Task<IActionResult> DetallesProducto(int id)
         {
             var detalles = await _context.Products.FindAsync(id);
             if (detalles == null) {
                 // Pasar un mensaje personalizado a la vista
                 ViewBag.Message = "No hay datos en el inventario";
                 return View("Inventario", new List<Product>()); // Pasa una lista vacía para evitar errores en la vista
             }
             return View("DetallesProducto", detalles);

         }
        [HttpPost]
         public async Task<IActionResult> EditarProducto(Product product)
         {
             Console.WriteLine($"ID recibido en el controlador (POST): {product.IdProduct}");

             if (product.IdProduct == 0)
             {
                 TempData["ErrorMessage"] = "No se recibió un ID válido.";
                 return RedirectToAction("Inventario");
             }

             try
             {
                 var editable = await _context.Products.FindAsync(product.IdProduct);
                 if (editable == null)
                 {
                     TempData["ErrorMessage"] = "Producto no encontrado.";
                     return RedirectToAction("Inventario");
                 }

                 // Actualizar los valores
                 editable.ProductName = product.ProductName;
                 editable.ProductProvider = product.ProductProvider;
                 editable.ProductPrices = product.ProductPrices;
                 editable.ProductStock = product.ProductStock;
                 editable.ProductBuyed = product.ProductBuyed;

                 await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Producto actualizado correctamente.";
                 return RedirectToAction("Inventario");
             }
             catch (Exception ex)
             {
                 TempData["ErrorMessage"] = $"Ocurrió un error: {ex.Message}";
                 return RedirectToAction("Inventario");
             }

         }
        


        [HttpGet]
        public IActionResult CrearProducto()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Supongamos que el nombre del usuario está almacenado en la propiedad Name del token
                ViewData["Username"] = User.Identity.Name;
            }

            return View(new List<ProductDTO>()); // Devuelve una lista vacía para el formulario
        }
        
                [HttpPost]
                public async Task<IActionResult>CrearProductos(List<ProductDTO> productos)
                {
                    if (productos == null || !productos.Any())
                    {
                        ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                        return View(productos);
                    }

                    try
                    {
                        // Convertir los DTO a entidades del modelo de datos
                        var productEntities = productos.Select(p => new Product
                        {
                            ProductName = p.ProductName,
                            ProductProvider = p.ProductProvider,
                            ProductPrices = p.ProductPrices,
                            ProductStock = p.ProductStock,
                            ProductBuyed  = p.ProductBuyed
                        }).ToList();

                        // Agregar los productos al contexto
                        await _context.Products.AddRangeAsync(productEntities);

                        // Guardar los cambios en la base de datos
                        await _context.SaveChangesAsync();

                        TempData["Mensaje"] = "Productos agregados exitosamente.";
                        return RedirectToAction("CrearProducto");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al insertar los productos.");
                        ModelState.AddModelError(string.Empty, "Ocurrió un error al insertar los productos.");
                        return View(productos);
                    }
                }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

