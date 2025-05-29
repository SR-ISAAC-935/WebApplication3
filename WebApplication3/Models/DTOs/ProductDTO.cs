namespace WebApplication3.Models.DTOs
{
    public class ProductDTO
    {
        public int IdProducto { get; set; }
        public string ProductName { get; set; }
        public string ProductProvider { get; set; }
        public decimal ProductPrices { get; set; }

        public decimal ProductBuyed { get; set; }
        public int Cantidad { get; set; }
        public int ProductStock { get; set; }
    }
}
