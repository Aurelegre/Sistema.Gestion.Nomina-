using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Permiso
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public int? Padre { get; set; }

    public virtual ICollection<Permiso> InversePadreNavigation { get; set; } = new List<Permiso>();

    public virtual Permiso? PadreNavigation { get; set; }
}
