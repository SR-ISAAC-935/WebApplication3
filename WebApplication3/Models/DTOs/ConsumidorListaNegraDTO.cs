namespace WebApplication3.Models.DTOs
{
    public class ConsumidorListaNegraDTO
    {
        public int IdConsumidor { get; set; }
        public List<DetalleListaNegraDTO> Productos { get; set; }
        public decimal DeudaTotal { get; set; }
    }
}
