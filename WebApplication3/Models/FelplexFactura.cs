namespace WebApplication3.Models
{
    public class FelplexFactura
    {
        public string type { get; set; } = "FACT";

 
        public string currency { get; set; } = "GTQ";
        public string datetime_issue { get; set; }
        public string external_id { get; set; } = $"LUMI-{DateTime.UtcNow:yyyyMMddHHmmss}";
        public List<Item> items { get; set; }
        public double total { get; set; }
        public double total_tax { get; set; }
        public int to_cf { get; set; } = 0;
        public required Receptor to { get; set; }
        public object exempt_phrase { get; set; } = null;
        public List<CustomField> custom_fields { get; set; }
    }
}
