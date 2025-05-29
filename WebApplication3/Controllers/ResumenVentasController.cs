using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    public class ResumenVentasController : Controller
    {
        public IActionResult ResumenHoy()
        {
            return View();
        }
        public IActionResult Vendido()
        {
            return View();
        }
    }
}
