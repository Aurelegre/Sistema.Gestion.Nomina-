using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class Nomina
{
    public int Id { get; set; }

    public int? IdEmpleado { get; set; }

    public decimal? SueldoExtra { get; set; }

    public decimal? Comisiones { get; set; }

    public decimal? Bonificaciones { get; set; }

    public decimal? OtrosIngresos { get; set; }

    public decimal? Igss { get; set; }

    public decimal? Isr { get; set; }

    public decimal? Prestamos { get; set; }

    public decimal? Anticipos { get; set; }

    public decimal? OtrosDesc { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }
}
