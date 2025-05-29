namespace WebApplication3.Models.DTOs
{
    public class ProductItem
    {
        public string Code { get; set; }
        public string ConsumerName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}
