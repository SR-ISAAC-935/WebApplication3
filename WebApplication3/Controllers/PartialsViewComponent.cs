using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Controllers
{
    public class PartialsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ListaNegraDTO();
            return View(model);
        }
    }
}
