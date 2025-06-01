using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class ConsuElectricista
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Consumidore> Consumidores { get; set; } = new List<Consumidore>();
}
