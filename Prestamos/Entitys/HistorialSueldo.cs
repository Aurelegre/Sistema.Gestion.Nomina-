using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class HistorialSueldo
{
    public int Id { get; set; }

    public decimal? NuevoSalario { get; set; }

    public decimal? AnteriorSalario { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdEmpleado { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
