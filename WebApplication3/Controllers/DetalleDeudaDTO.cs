namespace WebApplication3.Controllers
{
    public class DetalleDeudaDTO
    {
        public string consumidor { get; set; }
        public string electricista { get; set; }
        public DateOnly Fecha { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string status { get; set; }
        public decimal Total { get; set; }
    }
}