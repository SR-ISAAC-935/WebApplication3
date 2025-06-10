using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.DTOs
{
    public class VentasDTO
    {
        [Required(ErrorMessage = "El ID del consumidor es obligatorio.")]
        public int IdElectricista { get; set; }
        public string NombreConsumidor { get; set; }
        public string NombreElectricista { get; set; }
        public required string nit { get; set; }
        public required string direccion { get; set; }
        public required string tax_name { get; set; }
        public int idrole { get; set; } // Nuevo campo para el rol del usuario

        [Required(ErrorMessage = "El ID del consumidor es obligatorio.")]
        public int IdConsumidor { get; set; }

        [Required(ErrorMessage = "La deuda es obligatoria.")]
        [Range(0, double.MaxValue, ErrorMessage = "La deuda debe ser un valor positivo.")]
        public decimal Deuda { get; set; }
        [Required(ErrorMessage = "La fecha de venta es obligatoria.")]
        public DateTime FechaVenta { get; set; }


        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo.")]
        public decimal Precio { get; set; }
        public required string ProductName { get; set; }
    }
}
