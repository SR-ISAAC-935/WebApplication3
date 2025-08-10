using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WebApplication3.Controllers.Servicesxml;
using WebApplication3.Models.ModeladoFel;

namespace WebApplication3.Controllers
{
    public class XmlController : Controller
    {
        private readonly ServiceXmls _xmlService;
        public XmlController(ServiceXmls xmlService)
        {
            _xmlService = xmlService;
        }
        public async Task<IActionResult> VerFactura()
        {
            
             string url = "https://app.felplex.com/xml/203F2335-2BFF-4B73-A6F0-40FCF3A99B4F";
             var xmlDoc = await ObtenerFacturaDesdeXmlAsync(url);

             if (xmlDoc == null)
             {
                 return BadRequest("No se pudo obtener el XML.");
             }

             // Ejemplo: obtener datos específicos del XML
          Console.WriteLine("======= DATOS DE FACTURA =======");

             return View(); // o retorna json si quieres ver los datos: return Json(xmlDoc);
         }

        [HttpGet]
        public async Task<IActionResult> DescargarRecibo()
        {
            try
            {
                string url = "https://app.felplex.com/xml/203F2335-2BFF-4B73-A6F0-40FCF3A99B4F";
                var factura = await ObtenerFacturaDesdeXmlAsync(url);
                if (factura == null)
                {
                    return BadRequest("No se pudo obtener la factura desde el XML.");
                }
               
               using  var memoryStream = new MemoryStream(); // 👈 SIN using
                if(memoryStream.CanRead == false)
                {
                    Console.WriteLine("MemoryStream no se puede leer. Verifica la inicialización.");
                }
                else { Console.WriteLine("MemoryStream se puede leer correctamente."); }

                GenerarRecibo(factura, memoryStream);
               var buffer = memoryStream.ToArray();
                return File(new MemoryStream(buffer), "application/pdf", $"Recibo_{factura.NumeroFactura}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descargar el recibo: {ex.Message} - {ex.InnerException?.Message}");
                return BadRequest("Error al generar el recibo.");
            }
        }

        public static async Task<FacturaModelo> ObtenerFacturaDesdeXmlAsync(string urlXml)
        {
            using var httpClient = new HttpClient();
            var xmlContent = await httpClient.GetStringAsync(urlXml);

            var serializer = new XmlSerializer(typeof(GTDocumento));
            using var reader = new StringReader(xmlContent);
            var documento = (GTDocumento)serializer.Deserialize(reader);

            var datosEmision = documento.SAT.DTE.DatosEmision;
            var certificacion = documento.SAT.DTE.Certificacion;
            Console.WriteLine($" nombre comercial{documento.SAT.DTE.DatosEmision.Emisor.NombreComercial}");
            var factura = new FacturaModelo
            {
                NombreComercial =documento.SAT.DTE.DatosEmision.Emisor.NombreComercial,
                NombreEmisor = datosEmision.Emisor.NombreEmisor,
                NitEmisor = datosEmision.Emisor.NITEmisor,
                DireccionEmisor = $"{datosEmision.Emisor.Direccion.Calle?.Replace("undefined", "").Trim()}, " +
                  $"{datosEmision.Emisor.Direccion.Municipio?.Replace("undefined", "").Trim()} " +
                  $"{datosEmision.Emisor.Direccion.Departamento?.Replace("undefined", "").Trim()}",
                NombreReceptor = datosEmision.Receptor.Nombre,
                NitReceptor = datosEmision.Receptor.NIT,
                FechaEmision = datosEmision.DatosGenerales.FechaHoraEmision,
                NumeroAutorizacion= certificacion.NumeroAutorizacion.Autorizacion,
                NumeroFactura = certificacion.NumeroAutorizacion.Numero,
                Serie = certificacion.NumeroAutorizacion.Serie,
                NumeroAcceso = datosEmision.DatosGenerales.NumeroAcceso,
                Total = datosEmision.Totales.GranTotal,
                IVA = datosEmision.Totales.TotalImpuestos.TotalImpuesto.TotalMontoImpuesto,
                Items = new List<DetalleItem>()
            };

            foreach (var item in datosEmision.Items.ItemList)
            {
                factura.Items.Add(new DetalleItem
                {
                    Cantidad = item.Cantidad,
                    Descripcion = item.Descripcion,
                    PrecioUnitario = item.PrecioUnitario,
                    Total = item.Total
                });
            }

            return factura;
        }

        public static void GenerarRecibo(FacturaModelo factura, MemoryStream memoryStream)
        {
            try
            {
                memoryStream.CanRead.Equals(true);
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new iText.Layout.Document(pdf);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var headerTable = new Table(1).UseAllAvailableWidth(); // Una columna

                var headerCell = new Cell()
                    .SetBackgroundColor(new DeviceRgb(235, 239, 254))
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(3)
                   .SetFontColor(new DeviceRgb(25, 63, 230));

                // Agregar contenido como párrafos dentro de la celda
               
                headerCell.Add(new Paragraph($"{factura.NombreEmisor}                                                                              NÚMERO DE AUTORIZACIÓN: ")
                    .SetFont(font).SetFontSize(10));
                headerCell.Add(new Paragraph($"NIT Emisor: {factura.NitEmisor}                                                                         {factura.NumeroAutorizacion}")
                    .SetFont(font).SetFontSize(10));
                headerCell.Add(new Paragraph($"{factura.NombreComercial}                                                                                            Serie: {factura.Serie}  Número de DTE: {factura.NumeroFactura}")
              .SetFont(font).SetFontSize(10));
                headerCell.Add(new Paragraph($"{factura.DireccionEmisor}   ")
                    .SetFont(font).SetFontSize(10));
                headerTable.AddCell(headerCell);
                document.Add(headerTable);
                document.Add(new Paragraph($"NIT Receptor: {factura.NitReceptor}                                                                     Fecha y hora de emisión: {factura.FechaEmision:dd-MMM-yyyy HH:mm:ss}")
                   .SetFont(font).SetFontSize(10));
                document.Add(new Paragraph($"Nombre Receptor: {factura.NombreReceptor}          Fecha y hora de certificación: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}")
                    .SetFont(font).SetFontSize(10));
                document.Add(new Paragraph($"Moneda: GTQ")
                    .SetFont(font).SetFontSize(10).SetMarginBottom(10));
                // TABLA DE PRODUCTOS (Simula el documento estructurado SAT)
                var table = new Table(new float[] { 1, 5, 3, 3, 3 }).UseAllAvailableWidth();

                table.AddHeaderCell("No");
                table.AddHeaderCell("Descripción");
                table.AddHeaderCell("Cantidad");
                table.AddHeaderCell("P. Unitario (Q)");
                table.AddHeaderCell("Total (Q)");

                int index = 1;
                foreach (var item in factura.Items)
                {
                    table.AddCell(index.ToString());
                    table.AddCell(item.Descripcion);
                    table.AddCell(item.Cantidad.ToString());
                    table.AddCell(item.PrecioUnitario.ToString("F2"));
                    table.AddCell(item.Total.ToString("F2"));
                    index++;
                }

                document.Add(table);

                // TOTALES
                document.Add(new Paragraph($"TOTALES: Q{factura.Total:F2}    IVA: Q{factura.IVA:F6}")
                    .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT).SetMarginTop(10));

                // NOTA FINAL
                document.Add(new Paragraph("* Sujeto a pagos trimestrales ISR")
                    .SetFont(font).SetFontSize(9).SetMarginTop(10));

                // CERTIFICADOR
                document.Add(new Paragraph("Datos del certificador")
                    .SetFont(font).SetFontSize(10).SetMarginTop(20));
                document.Add(new Paragraph("CARI LATINOAMERICA, S.A.  NIT: 96941243")
                    .SetFont(font).SetFontSize(9));
                document.Add(new Paragraph("“Contribuyendo por el país que todos queremos”")
                    .SetFont(font).SetFontSize(9));

                
                document.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el recibo: {ex.Message} - {ex.InnerException?.Message}");
                throw;
            }
        }

    }
}
