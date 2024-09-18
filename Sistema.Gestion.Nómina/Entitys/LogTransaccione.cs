using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class LogTransaccione
{
    public int Id { get; set; }

    public int? IdEmpleado { get; set; }

    public int? IdEmpresa { get; set; }

    public string? Metodo { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Usuario { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }
}
