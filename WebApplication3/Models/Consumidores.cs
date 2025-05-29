namespace WebApplication3.Models;

public partial class Consumidores
{
    public int IdConsumidor { get; set; }

    public string NombreConsumidor { get; set; } = null!;

    public virtual ICollection<ListaNegra> ListaNegras { get; set; } = new List<ListaNegra>();
}
