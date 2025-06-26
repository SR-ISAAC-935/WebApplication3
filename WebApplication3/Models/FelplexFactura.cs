namespace WebApplication3.Models
{
    public class FelplexFactura
    {
        public string type { get; set; } = "FACT";
        public string tax_code_type { get; set; } = "NIT";
        public string currency { get; set; } = "GTQ";
        public string datetime_issue { get; set; }
        public string external_id { get; set; } = $"LUMI-{DateTime.UtcNow:yyyyMMddHHmmss}";
        public List<Item> items { get; set; }
        public double total { get; set; }
        public double total_tax { get; set; }
        public int? to_cf { get; set; } 
        public Receptor to { get; set; } 

        public List<CustomField> custom_fields { get; set; }
    }
}
