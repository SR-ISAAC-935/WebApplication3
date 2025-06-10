
namespace WebApplication3.Models
{
    public class FacturaModel
    {
        public string Emisor { get; set; }
        public string Receptor { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public List<FraseModel> Frases { get; set; }
        public IEnumerable<Producto> Productos { get; internal set; }

    }
}
