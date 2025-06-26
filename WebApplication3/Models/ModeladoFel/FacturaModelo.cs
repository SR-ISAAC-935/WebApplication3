namespace WebApplication3.Models.ModeladoFel
{
    public class FacturaModelo
    {
        public string NombreEmisor { get; set; }
        public string NitEmisor { get; set; }
        public string DireccionEmisor { get; set; }
        public string NombreReceptor { get; set; }
        public string NitReceptor { get; set; }
        public DateTime FechaEmision { get; set; }
        public string NumeroFactura { get; set; }
        public string Serie { get; set; }
        public string NumeroAcceso { get; set; }
        public decimal Total { get; set; }
        public decimal IVA { get; set; }
        public List<DetalleItem> Items { get; set; }
    }
}
