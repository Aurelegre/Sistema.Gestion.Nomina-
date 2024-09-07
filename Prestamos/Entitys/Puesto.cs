using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Puesto
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }

    public int? IdDepartamento { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Departamento? IdDepartamentoNavigation { get; set; }
}
