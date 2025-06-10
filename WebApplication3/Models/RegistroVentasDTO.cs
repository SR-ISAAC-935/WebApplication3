namespace WebApplication3.Models
{
    public class RegistroVentasDTO
    {
        public int IdSales { get; set; }
        public int IdConsumidor { get; set; }
        public decimal Deuda { get; set; }
        public DateOnly FechaVenta { get; set; }
        public string NombreConsumidor { get; set; }
        
        public decimal total { get; set; }
    }
}
