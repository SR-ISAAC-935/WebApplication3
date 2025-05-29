namespace WebApplication3.Models.DTOs
{
    public class DeudaPendienteDTO
    {
        public int IdConsumidor { get; set; }
        public string NombreConsumidor { get; set; }
        public decimal Deuda { get; set; }
        public int IdListado { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string EstadoPago { get; set; }
    }

}
