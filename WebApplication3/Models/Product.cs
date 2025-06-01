using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductProvider { get; set; }

    public decimal ProductPrices { get; set; }

    public int ProductStock { get; set; }

    public decimal? ProductBuyed { get; set; }

    public virtual ICollection<DetailSalesClient> DetailSalesClients { get; set; } = new List<DetailSalesClient>();

    public virtual ICollection<DetalleListaNegra> DetalleListaNegras { get; set; } = new List<DetalleListaNegra>();
}
