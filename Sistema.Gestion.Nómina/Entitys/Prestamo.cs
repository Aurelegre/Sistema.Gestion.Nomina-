using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class Prestamo
{
    public int Id { get; set; }

    public int? IdEmpleado { get; set; }

    public decimal? Total { get; set; }

    public int? Cuotas { get; set; }

    public int? CuotasPendientes { get; set; }

    public decimal? TotalPendiente { get; set; }

    public int? Pagado { get; set; }

    public DateTime? FechaPrestamo { get; set; }

    public int? IdTipo { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual TiposPrestamo? IdTipoNavigation { get; set; }
}
