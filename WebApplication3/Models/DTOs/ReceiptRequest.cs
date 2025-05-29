namespace WebApplication3.Models.DTOs
{
    public class ReceiptRequest
    {
        public string Fecha { get; set; }
        public List<ReceiptItem> Registros { get; set; }
        public decimal Total { get; set; }
    }
}
