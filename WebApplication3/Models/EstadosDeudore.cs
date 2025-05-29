namespace WebApplication3.Models;

public partial class EstadosDeudore
{
    public int IdEstado { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<ListaNegra> ListaNegras { get; set; } = new List<ListaNegra>();
}
