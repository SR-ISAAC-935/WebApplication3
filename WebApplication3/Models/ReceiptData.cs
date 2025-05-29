using WebApplication3.Models.DTOs;

namespace WebApplication3.Models
{
    public class ReceiptData
    {
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public DateTime Date { get; set; }
        public List<ProductItem> Products { get; set; }
        public decimal Total { get; set; }
    }
}
