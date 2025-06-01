using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication3.Models.DTOs
{
    public class ProductoViewModel
    {
        public int IdProveedorSeleccionado { get; set; }

        public List<SelectListItem> Proveedores { get; set; } = new();
    }
}
