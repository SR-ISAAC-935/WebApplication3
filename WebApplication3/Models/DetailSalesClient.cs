namespace WebApplication3.Models;

public partial class DetailSalesClient
{
    public int IdSale { get; set; }

    public int IdVenta { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateOnly FechaDetalle { get; set; }

    public virtual Product IdProductNavigation { get; set; } = null!;

    public virtual SalesClientResume IdVentaNavigation { get; set; } = null!;
}
