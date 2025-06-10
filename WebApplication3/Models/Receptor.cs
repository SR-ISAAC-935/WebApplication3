namespace WebApplication3.Models
{
    public class Receptor
    {
        public string tax_code_type { get; set; }
        public string tax_code { get; set; }
        public string tax_name { get; set; }
        public Address address { get; set; }
    }
}