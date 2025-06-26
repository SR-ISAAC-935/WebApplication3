namespace WebApplication3.Models
{
    public class ListadoNegraDTO
    {
        public int IdListaNegra { get; set; }
        public int IdConsumidor { get; set; }
        public string NombreConsumidor { get; set; }
        public string NombreElectricista { get; set; }
        public decimal Deuda { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdEstado { get; set; }
        public string Estado { get; set; }
    }
}
