namespace WebApplication3.Models.DTOs
{
    public class ProductoPendienteDTO
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string EstadoPago { get; set; }
        public decimal Total { get; set; }  // Propiedad para el total
        public int IdConsumidor { get; set; }
        public string NombreConsumidor { get; set; }
        public int IdListado { get; set; }
        public int IdProducto { get; set; }
    }

}
