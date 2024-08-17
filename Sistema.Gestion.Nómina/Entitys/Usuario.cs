using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class Usuario
{
    public int Id { get; set; }

    public string? Usuario1 { get; set; }

    public string? Contraseña { get; set; }

    public int? IdRol { get; set; }

    public int? IdEmpresa { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual Role? IdRolNavigation { get; set; }
}
