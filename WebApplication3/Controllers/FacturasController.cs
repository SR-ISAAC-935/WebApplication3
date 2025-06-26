using Azure;
using iText.Commons.Bouncycastle.Asn1.X509;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using WebApplication3.Custom;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;
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

                Console.WriteLine(JsonConvert.SerializeObject(factura, Formatting.Indented));

                var jsonBody = JsonConvert.SerializeObject(factura, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });


                using var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://app.felplex.com/api/entity/5681/invoices/await");

                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("X-Authorization", "LnSTDWslTbmJ0LdoQJCy4fSkXiEwKayI3T26Q9fnzolpjdbCooCfEJzktm538riI");
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

        public async Task<FacturaModel> LeerFacturaDesdeXmlUrl(string xmlUrl)
    {
        var httpClient = new HttpClient();
        var xmlString = await httpClient.GetStringAsync(xmlUrl);

        var xml = XDocument.Parse(xmlString);

        var factura = new FacturaModel();

        // Suponiendo estructura: <DatosGenerales Emision="..." NumeroAcceso="..." />
        var datosGenerales = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "DatosGenerales");
        if (datosGenerales != null)
        {
            factura.FechaEmision = DateTime.Parse(datosGenerales.Attribute("FechaHoraEmision")?.Value ?? "");
            factura.NumeroFactura = datosGenerales.Attribute("NumeroAcceso")?.Value;
        }

        // Suponiendo estructura: <Frase TipoFrase="..." CodigoEscenario="..." />
        factura.Frases = xml.Descendants()
            .Where(x => x.Name.LocalName == "Frase")
            .Select(x => new FraseModel
            {
                TipoFrase = x.Attribute("TipoFrase")?.Value,
                CodigoEscenario = x.Attribute("CodigoEscenario")?.Value
            })
            .ToList();

        // Suponiendo estructura: <Emisor NombreComercial="..." />
        var emisor = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Emisor");
        factura.Emisor = emisor?.Attribute("NombreComercial")?.Value;

        // Suponiendo estructura: <Receptor Nombre="..." />
        var receptor = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Receptor");
        factura.Receptor = receptor?.Attribute("Nombre")?.Value;

        // Suponiendo: <Totales GranTotal="..." />
        var totales = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Totales");
        if (totales != null)
        {
            factura.Total = decimal.Parse(totales.Attribute("GranTotal")?.Value ?? "0");
        }

        return factura;
    }
        public static void GenerarRecibo(FacturaModel factura, MemoryStream memoryStream)
        {
            try
            {
                // Crear el documento PDF
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                // Cargar la fuente estándar
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Título (puedes agregar uno si lo necesitas)
                Paragraph titulo = new Paragraph("RECIBO DE FACTURA")
                    .SetFont(font)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER);
                document.Add(titulo);

                // Fecha de emisión
                Paragraph fechaTexto = new Paragraph($"Fecha: {factura.FechaEmision:dd/MM/yyyy}")
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT);
                document.Add(fechaTexto);

                // Nombre del cliente
                Paragraph cliente = new Paragraph($"Cliente: {factura.Receptor}")
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetMarginTop(10);
                document.Add(cliente);

                // Dirección fija (puedes cambiarla si la sacas del XML)
                Paragraph direccion = new Paragraph("Dirección: Asunción Mita, Jutiapa")
                    .SetFont(font)
                    .SetFontSize(10);
                document.Add(direccion);

                // Tabla de productos
                Table table = new Table(new float[] { 1, 5, 3, 3 }).UseAllAvailableWidth();

                table.AddHeaderCell("Cant.");
                table.AddHeaderCell("Descripción");
                table.AddHeaderCell("P. Unitario");
                table.AddHeaderCell("Importe");

                foreach (var item in factura.Productos)
                {
                    decimal importe = item.Cantidad* item.PrecioUnitario;

                    table.AddCell(item.Cantidad.ToString());
                    table.AddCell(item.Descripcion ?? "Producto sin descripción");
                    table.AddCell(item.PrecioUnitario.ToString("F2"));
                    table.AddCell(importe.ToString("F2"));
                }

                document.Add(table);

                // Total
                decimal total = factura.Productos.Sum(p => p.Cantidad * p.PrecioUnitario);
                Paragraph totalTexto = new Paragraph($"Total: Q {total:F2}")
                    .SetFont(font)
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT);
                document.Add(totalTexto);

                // Frases (opcional)
                if (factura.Frases?.Any() == true)
                {
                    Paragraph frase = new Paragraph("Frases:")
                        .SetFont(font)
                        .SetFontSize(9)
                        .SetMarginTop(10);
                    document.Add(frase);

                    foreach (var f in factura.Frases)
                    {
                        Paragraph p = new Paragraph($"- TipoFrase: {f.TipoFrase}, Escenario: {f.CodigoEscenario}")
                            .SetFont(font)
                            .SetFontSize(9);
                        document.Add(p);
                    }
                }

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
