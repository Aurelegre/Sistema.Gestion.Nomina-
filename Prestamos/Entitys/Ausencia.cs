using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Ausencia
{
    public int Id { get; set; }

    public int? IdEmpleado { get; set; }

    public DateTime? FechaSolicitud { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? TotalDias { get; set; }

    public int? Autorizado { get; set; }

    public int? Deducible { get; set; }

    public string? Detalle { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
