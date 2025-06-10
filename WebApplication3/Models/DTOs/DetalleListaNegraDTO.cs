namespace WebApplication3.Models.DTOs
{
    public class DetalleListaNegraDTO
    {
        public int IdConsumidor { get; set; }
        public int idComprador { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public int IdEstado { get; set; }

        public decimal Deuda { get; set; }
    }
}
