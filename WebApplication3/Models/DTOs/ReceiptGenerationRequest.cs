namespace WebApplication3.Models.DTOs
{
    public class ReceiptGenerationRequest
    {
        public string Fecha { get; set; }
        public string ConsumidoresJson { get; set; }
        public decimal DeudaTotal { get; set; }
    }
}
