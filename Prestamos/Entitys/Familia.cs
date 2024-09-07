using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Familia
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public int? Edad { get; set; }

    public string? Parentesco { get; set; }

    public int? IdEmpleado { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
