using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Abono
{
    public int Id { get; set; }

    public int FacturaDeuda { get; set; }

    public int IdUsuario { get; set; }

    public decimal AbonoDeuda { get; set; }

    public DateTime FechaAbono { get; set; }

    public virtual ListaNegra FacturaDeudaNavigation { get; set; } = null!;

    public virtual Consumidore IdUsuarioNavigation { get; set; } = null!;
}
