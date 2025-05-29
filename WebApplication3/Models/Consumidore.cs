namespace WebApplication3.Models;

public partial class Consumidore
{
    public int IdConsumidor { get; set; }

    public string NombreConsumidor { get; set; } = null!;

    public virtual ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public virtual ICollection<ListaNegra> ListaNegras { get; set; } = new List<ListaNegra>();

    public virtual ICollection<SalesClientResume> SalesClientResumes { get; set; } = new List<SalesClientResume>();
}
