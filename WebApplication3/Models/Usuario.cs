using System;
using System.Collections.Generic;

namespace WebApplication3.Models;

public partial class Usuario
{
    public int IdUser { get; set; }

    public string Users { get; set; } = null!;

    public string? Passwords { get; set; }

    public int? IdRole { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public virtual Role? IdRoleNavigation { get; set; }
}
