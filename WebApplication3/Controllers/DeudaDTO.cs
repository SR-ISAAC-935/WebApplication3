
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace WebApplication3.Controllers
{
    public class DeudaDTO
    {
        public int IdElectricista { get; set; }
        public string electricista { get; set; }
        public string consumidor { get; set; }
        public int IdListado { get; internal set; }
        public decimal Deuda { get; internal set; }
        public String idEstado { get; internal set; }
        public DateTime FechaVenta { get; internal set; }
    }
}