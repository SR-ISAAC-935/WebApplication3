using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class SalesClientResume
{
    public int IdSales { get; set; }

    public int IdUsuario { get; set; }

    public string NombreConsumidor { get; set; } = null!;
    public string NombreElectricista { get; set; } = null!;
    public decimal Total { get; set; }

    public DateOnly FechaVenta { get; set; }

    public int? IdConsumidor { get; set; }

    public virtual ICollection<DetailSalesClient> DetailSalesClients { get; set; } = new List<DetailSalesClient>();

    public virtual Consumidore IdUsuarioNavigation { get; set; } = null!;
}
