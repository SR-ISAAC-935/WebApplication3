namespace WebApplication3.Models.DTOs
{
    public class SalesClientResumeDTO
    {
        public int IdSales { get; set; }

        public string NombreCOnsumidor { get; set; }

        public string nombreElectricista { get; set; }  
       
        public decimal Total { get; set; }

        public DateOnly FechaVenta { get; set; }

    }
}
