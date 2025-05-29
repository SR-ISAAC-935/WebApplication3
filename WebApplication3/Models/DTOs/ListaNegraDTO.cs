using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.DTOs
{
    public class ListaNegraDTO
    {
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

        [Required(ErrorMessage = "El estado del deudor es obligatorio.")]
        public int IdEstado { get; set; } // Nuevo campo

    }


}
