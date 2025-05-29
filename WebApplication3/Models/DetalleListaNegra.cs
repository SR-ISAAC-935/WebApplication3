namespace WebApplication3.Models;

public partial class DetalleListaNegra
{
    public int IdDeuda { get; set; }

    public int IdListado { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public int IdEstado { get; set; }

    public DateOnly FechaCompra { get; set; }

    public virtual EstadoProducto IdEstadoNavigation { get; set; } = null!;

    public virtual ListaNegra IdListadoNavigation { get; set; } = null!;

    public virtual Product IdProductoNavigation { get; set; } = null!;
}
