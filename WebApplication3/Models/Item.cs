using Org.BouncyCastle.Bcpg.OpenPgp;

namespace WebApplication3.Models
{
    public class Item
    {
        public string qty { get; set; }

        public char type { get; set; } = 'b'; // P for product, S for service
        public double price { get; set; }
        public string description { get; set; }
        public int without_iva { get; set; } = 0;
        public double discount { get; set; } = 0;
        public int is_discount_percentage { get; set; } = 0;
       
    }
}
