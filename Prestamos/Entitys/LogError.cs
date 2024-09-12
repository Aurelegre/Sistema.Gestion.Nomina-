using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class LogError
{
    public int Id { get; set; }

    public int? IdEmpleado { get; set; }

    public string? Metodo { get; set; }

    public string? Descripcion { get; set; }

    public string? Error { get; set; }

    public string? StackTrace { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdEmpresa { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }
}
