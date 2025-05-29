namespace WebApplication3.Models.DTOs
{
    public class RegistroDTO
    {
        public int IdConsumidor { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Deuda { get; set; }
        public DateTime FechaVenta { get; set; }
    }
}
