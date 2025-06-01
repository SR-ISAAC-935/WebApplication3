using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class ListaNegra
{
    public int IdListado { get; set; }

    public int IdConsumidor { get; set; }

    public decimal Deuda { get; set; }

    public DateTime FechaVenta { get; set; }

    public int? IdEstado { get; set; }

    public virtual ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public virtual ICollection<DetalleListaNegra> DetalleListaNegras { get; set; } = new List<DetalleListaNegra>();

    public virtual Consumidore IdConsumidorNavigation { get; set; } = null!;

    public virtual EstadosDeudore? IdEstadoNavigation { get; set; }
}
