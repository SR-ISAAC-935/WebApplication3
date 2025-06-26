using Microsoft.AspNetCore.Mvc;
using WebApplication3.Controllers.Servicesxml;

namespace WebApplication3.Controllers
{
    public class XmlController : Controller
    {
        private readonly ServiceXmls _xmlService;
        public XmlController(ServiceXmls xmlService)
        {
            _xmlService = xmlService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> VerFactura()
        {
            string url = "https://felplex.stage.plex.lat/xml/D82C53AA-1189-41E0-BA80-60583F5A92D7";
            var xmlDoc = await _xmlService.ObtenerXmlDesdeUrlAsync(url);

            if (xmlDoc == null)
            {
                return BadRequest("No se pudo obtener el XML.");
            }

            // Ejemplo: obtener datos específicos del XML
            var emisor = xmlDoc.Descendants("Emisor").FirstOrDefault()?.Value;
            var receptor = xmlDoc.Descendants("Receptor").FirstOrDefault()?.Value;

            ViewBag.Emisor = emisor;
            ViewBag.Receptor = receptor;

            return View(xmlDoc); // o retorna json si quieres ver los datos: return Json(xmlDoc);
        }
    }
}
