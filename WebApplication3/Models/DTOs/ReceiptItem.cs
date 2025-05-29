namespace WebApplication3.Models.DTOs
{
    public class ReceiptItem
    {
        public string Consumidor { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Deuda { get; set; }
    }
}
