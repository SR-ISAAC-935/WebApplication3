using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class CotizacionController : Controller
    {

        private readonly ILogger<CotizacionController> _logger;
        LumitecContext _context;
        public CotizacionController(ILogger<CotizacionController> logger, LumitecContext lumitecContext)
        {
            _logger = logger;
            _context = lumitecContext;
        }
        public IActionResult Cotizaciones()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ImprimirRecibos([FromBody] ReciboDTO reciboData)
        {
            try
            {
                // Crear un MemoryStream para almacenar el PDF en memoria
                using (var memoryStream = new MemoryStream())
                {
                    // Generar el recibo usando la clase ReciboGenerator
                    GenerarRecibo(
                        reciboData.NombreCliente,
                        reciboData.Fecha,
                        reciboData.Productos.Select(p => new Producto
                        {
                            Descripcion = p.Descripcion,
                            Cantidad = p.Cantidad,
                            PrecioUnitario = p.PrecioUnitario
                        }).ToList(),
                        memoryStream
                    );

                    // Devolver el PDF como respuesta
                    return File(memoryStream.ToArray(), "application/pdf", "Recibo.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar el recibo: {ex.Message}");
            }
        }

        // Actualizar la función GenerarRecibo para aceptar un MemoryStream
        public static void GenerarRecibo(string nombreCliente, DateTime fecha,
                                      List<Producto> productos, MemoryStream memoryStream)
        {
            try
            {
                // Crear el documento PDF
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                // Cargar la fuente estándar
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Agregar título y encabezado
                Paragraph titulo = new Paragraph("")
                    .SetFont(font)
                    .SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(titulo);
                Paragraph fechaTexto = new Paragraph($" {fecha.ToString("dd/MM/yyyy")}")
                    .SetFont(font)
                    .SetFontSize(17)
                    .SetMarginTop(17)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                document.Add(fechaTexto);

                Paragraph cliente = new Paragraph($"{nombreCliente ?? "Sin nombre"}")
                    .SetFont(font)
                    .SetMarginLeft(60)
                    .SetMarginTop(-10)
                    .SetFontSize(10);
                document.Add(cliente);

                Paragraph direccion = new Paragraph("Asunción Mita, Jutiapa")
                    .SetFont(font)
                    .SetMarginLeft(75)
                    .SetMarginTop(-5)
                    .SetFontSize(10);
                document.Add(direccion);

                // Crear tabla sin bordes visibles
                // Crear la tabla con anchos relativos
                Table table = new Table(new float[] { 1, 5, 3, 3 })  // Ajustamos el tamaño relativo de las columnas
                    .SetMarginLeft(60)  // Ajustamos margen izquierdo para alinear la tabla bajo la dirección
                    .SetMarginTop(15)
                    .UseAllAvailableWidth()
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);  // Puedes usar LEFT si prefieres

                // Agregar los encabezados
                table.AddHeaderCell(new Cell().Add(new Paragraph("").SetFont(font).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER));

                table.AddHeaderCell(new Cell().Add(new Paragraph("").SetFont(font).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginLeft(-10)
                    .SetBorder(Border.NO_BORDER));

                table.AddHeaderCell(new Cell().Add(new Paragraph("").SetFont(font).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER));

                table.AddHeaderCell(new Cell().Add(new Paragraph("").SetFont(font).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER));

                // Contenido de las celdas de la tabla
                foreach (var producto in productos)
                {
                    string descripcion = producto.Descripcion ?? "Producto sin descripción";
                    int cantidad = producto.Cantidad;
                    decimal precioUnitario = producto.PrecioUnitario;
                    decimal importe = cantidad * precioUnitario;

                    // Alineación a la derecha en las columnas de "Precio Unitario" e "Importe"
                    table.AddCell(new Cell().Add(new Paragraph(cantidad.ToString())
                        .SetFont(font)
                        .SetFontSize(10))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginLeft(1)
                        .SetBorder(Border.NO_BORDER));

                    table.AddCell(new Cell().Add(new Paragraph(descripcion)
                        .SetFont(font)
                        .SetFontSize(10))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginLeft(-15)
                        .SetBorder(Border.NO_BORDER));

                    table.AddCell(new Cell().Add(new Paragraph(precioUnitario.ToString("F2"))
                        .SetFont(font)
                        .SetFontSize(10))
                        .SetTextAlignment(TextAlignment.RIGHT)  // Alineación a la derecha
                        .SetMarginLeft(500)  // Esto alinea con la fecha
                        .SetBorder(Border.NO_BORDER));

                    table.AddCell(new Cell().Add(new Paragraph(importe.ToString("F2"))
                        .SetFont(font)
                        .SetFontSize(10))
                        .SetTextAlignment(TextAlignment.RIGHT)  // Alineación a la derecha
                        .SetMarginLeft(225)  // Esto alinea con la fecha
                        .SetBorder(Border.NO_BORDER));
                }

                // Agregar la tabla al documento
                document.Add(table);

                // Página actual


                // Coordenadas fijas: x (derecha), y (altura desde abajo), ancho (para que no se corte el texto)
                // Página actual
                int paginaActual = pdfDocument.GetNumberOfPages();

                // Coordenadas fijas: x (derecha), y (altura desde abajo), ancho (para que no se corte el texto)
                decimal total = productos.Sum(p => p.Cantidad * p.PrecioUnitario);
                Paragraph totalTexto = new Paragraph($"Q {total.ToString("F2")}")
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetFixedPosition(paginaActual, 475, 475, width: 100); // ajusta 'x' e 'y' según el diseño

                document.Add(totalTexto);



                // Cerrar el documento
                document.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar generar el recibo: {ex.Message} - {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}



