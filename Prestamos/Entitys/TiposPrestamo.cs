using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class TiposPrestamo
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
