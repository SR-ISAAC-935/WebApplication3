namespace WebApplication3.Models;

public partial class EstadoProducto
{
    public int IdEstado { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<DetalleListaNegra> DetalleListaNegras { get; set; } = new List<DetalleListaNegra>();
}
