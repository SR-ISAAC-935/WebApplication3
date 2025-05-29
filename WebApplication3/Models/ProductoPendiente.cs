namespace WebApplication3.Models
{
    public partial class ProductoPendiente
    {
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string EstadoPago { get; set; }
    }
}
