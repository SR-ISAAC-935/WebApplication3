namespace WebApplication3.Models
{
    public class FelplexResponse
    {
        public string invoice_url { get; set; }
        public string invoice_xml { get; set; }
        public bool valid { get; set; }
        public object uuid { get; set; }
        public object errors { get; set; }
    }
}
