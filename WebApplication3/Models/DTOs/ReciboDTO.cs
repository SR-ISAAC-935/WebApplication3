namespace WebApplication3.Models.DTOs
{
    public class ReciboDTO
    {
        public string NombreCliente { get; set; }
        public string Direccion { get; set; }
        public DateTime Fecha { get; set; }
        public List<ProductoDTO> Productos { get; set; }
    }
}
