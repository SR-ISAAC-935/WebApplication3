using Azure;
using iText.Commons.Bouncycastle.Asn1.X509;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using WebApplication3.Custom;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;
using WebApplication3.Models.ModeladoFel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication3.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        public async Task<IActionResult> Facturacion()
        {
            const string FegoraTokenKey = "FegoraToken";

            // Verifica si ya hay un token guardado en sesión
            var tokenSesion = HttpContext.Session.GetString(FegoraTokenKey);
            if (!string.IsNullOrEmpty(tokenSesion))
            {
                Console.WriteLine($" devuelveme este token {tokenSesion}");
            }

            var token = await Sesiones();
            ViewBag.Token = token;
            return View();

        }

        public async Task<string> Sesiones()
        {
            const string FegoraTokenKey = "FegoraToken";

            // Intenta obtener el token de la cookie primero
            var tokenCookie = HttpContext.Request.Cookies["FegoraToken"];

            if (!string.IsNullOrEmpty(tokenCookie))
            {
                // Si la cookie existe, la devolvemos y también la guardamos en la sesión (opcional, para la sesión actual)
                HttpContext.Session.SetString(FegoraTokenKey, tokenCookie);
                return $"Token recuperado de la cookie: {tokenCookie}";
            }

            // Si no hay token en la cookie, o la cookie no existe, solicita uno nuevo
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fegora.com/token");
            request.Headers.Add("Accept", "application/json");

            var collection = new List<KeyValuePair<string, string>>
    {
        new("grant_type", "password"),
        new("username", "Isaac7Guerra@gmail.com"),
        new("password", "Ara16081976"),
        new("client_id", "apiApp")
    };

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseBody);
            var newToken = json.RootElement.GetProperty("access_token").GetString();

            // Guarda el NUEVO token en la cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            };
            HttpContext.Response.Cookies.Append("FegoraToken", newToken, cookieOptions);

            // Guarda el NUEVO token en la sesión del servidor
            HttpContext.Session.SetString(FegoraTokenKey, newToken);

            return $"Este es el nuevo token: {newToken}";
        }

        [HttpGet]
        public async Task<IActionResult> ConsultaNit(string nit)
        {
            if (nit == null)
            {
                Console.WriteLine(nit);
            }
            else {
                Console.WriteLine("yolo no vacio");
            }
            try
            {
                var client = new HttpClient();
                var url = $"https://app.felplex.com/api/entity/5681/find/NIT/{nit}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-Authorization", "LnSTDWslTbmJ0LdoQJCy4fSkXiEwKayI3T26Q9fnzolpjdbCooCfEJzktm538riI");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Devuelve directamente el JSON como arreglo
                return Content(jsonResponse, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al consultar el NIT", detalles = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Documento([FromBody] CrearVentaRequest request)
        {
            if (request == null)
                return StatusCode(500, new { mensaje = "No se envió nada" });

            try
            {
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")
                );

                var random = new Random();
                int randomNumber = random.Next(2, 10001);
                string paddedNumber = randomNumber.ToString("D5");
                string ampm = localTime.Hour < 12 ? "am" : "pm";

                string external_id = $"LUMI-{paddedNumber}{localTime:yyyyMMddHHmmss}{ampm}";

                var detalles = JsonConvert.DeserializeObject<List<VentasDTO>>(request.ConsumidoresJson);

                if (detalles == null || detalles.Count == 0)
                    return BadRequest(new { mensaje = "No se encontraron detalles válidos para facturar." });

                double total = detalles.Sum(d => (double)d.Deuda);
                double total_tax = total * 0.12;

                var factura = new FelplexFactura
                {
                    datetime_issue = localTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    external_id = external_id,
                    items = detalles.Select(d => new Item
                    {
                        qty = d.Cantidad.ToString(),
                        type = 'B', // asegurarse que sea string
                        price = (double)d.Precio,
                        description = d.ProductName
                    }).ToList(),
                    total = total,
                    total_tax = total_tax,
                    custom_fields = new List<CustomField>
            {
                new CustomField { name = "IVA total incluido", value = total_tax.ToString("F2") }
            }
                };

                // 👇 Verifica si hay NIT válido
                if (string.IsNullOrEmpty(detalles[0].nit) || detalles[0].nit == "0")
                {
                    factura.to_cf = 1; // Consumidor final

                    factura.to = new Receptor
                    {
                        tax_code_type = "NIT",
                        address = new Address
                        {
                            city = detalles[0].direccion
                        }
                    };
                }
                else
                {
                    factura.to_cf = 0; // No es consumidor final
                    factura.to = new Receptor
                    {
                        tax_code_type = "NIT",
                        tax_code = detalles[0].nit,
                        tax_name = detalles[0].tax_name,
                        address = new Address
                        {
                            city = detalles[0].direccion
                        }
                    };
                }

                var jsonBody = JsonConvert.SerializeObject(factura, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                Console.WriteLine(jsonBody);

                using var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://felplex.stage.plex.lat/api/entity/392/invoices/await");
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("X-Authorization", "YHuBX63N5F4uYQKwWXNjb7Mnw0PegrwyoiBFwo2wdUAbspYzk0fG4SOIVppuz5pk");
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(req);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("respuesta" + response.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var felplexResponse = JsonConvert.DeserializeObject<FelplexResponse>(responseContent);

                    Console.WriteLine("PDF: " + felplexResponse.invoice_url);
                    Console.WriteLine("XML: " + felplexResponse.invoice_xml);

                    var fileName = string.Concat(detalles[0].tax_name?.Where(c => !Path.GetInvalidFileNameChars().Contains(c)) ?? "Factura");

                    var pdfBytes = await client.GetByteArrayAsync(felplexResponse.invoice_url);
                    await System.IO.File.WriteAllBytesAsync($"{fileName}.pdf", pdfBytes);

                   
                    return Ok(new
                    {
                        mensaje = "Factura enviada correctamente",
                        pdf = felplexResponse.invoice_url,
                        xml = felplexResponse.invoice_xml,
                        
                    });
                }
                else
                {
                    Console.WriteLine("Error al enviar la factura: " + responseContent);
                    return StatusCode((int)response.StatusCode, new { mensaje = "Error al enviar la factura", detalles = responseContent });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { mensaje = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult>DescargarRecibo(string xmlurl)
        {
            try
            {
                string url = xmlurl;
                var factura = await ObtenerFacturaDesdeXmlAsync(url);
                if (factura == null)
                {
                    return BadRequest("No se pudo obtener la factura desde el XML.");
                }

                using var memoryStream = new MemoryStream(); // 👈 SIN using
                if (memoryStream.CanRead == false)
                {
                    Console.WriteLine("MemoryStream no se puede leer. Verifica la inicialización.");
                }
                else { Console.WriteLine("MemoryStream se puede leer correctamente."); }

                GenerarRecibo(factura, memoryStream);
                var buffer = memoryStream.ToArray();
                return File(new MemoryStream(buffer), "application/pdf", $"Recibo_{factura.NumeroFactura}.pdf");
                // return File(new MemoryStream(buffer), "application/pdf", $"Recibo_{factura.NumeroFactura}.pdf");
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

            var factura = new FacturaModelo
            {

                NombreEmisor = datosEmision.Emisor.NombreEmisor,
                NitEmisor = datosEmision.Emisor.NITEmisor,
                DireccionEmisor = $"{datosEmision.Emisor.Direccion.Calle}, {datosEmision.Emisor.Direccion.Municipio}, {datosEmision.Emisor.Direccion.Departamento}",
                NombreReceptor = datosEmision.Receptor.Nombre,
                NitReceptor = datosEmision.Receptor.NIT,
                FechaEmision = datosEmision.DatosGenerales.FechaHoraEmision,
                NumeroAutorizacion = certificacion.NumeroAutorizacion.Autorizacion,
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

                headerCell.Add(new Paragraph($"{factura.DireccionEmisor}   Serie: {factura.Serie}  Número de DTE: {factura.NumeroFactura}")
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

