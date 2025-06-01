using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Consumidore
{
    public int IdConsumidor { get; set; }

    public string NombreConsumidor { get; set; } = null!;

    public int? IdRole { get; set; }

    public virtual ICollection<Abono> Abonos { get; set; } = new List<Abono>();

    public virtual ConsuElectricista? IdRoleNavigation { get; set; }

    public virtual ICollection<ListaNegra> ListaNegras { get; set; } = new List<ListaNegra>();

    public virtual ICollection<SalesClientResume> SalesClientResumes { get; set; } = new List<SalesClientResume>();
}
