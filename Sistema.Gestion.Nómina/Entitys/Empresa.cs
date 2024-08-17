using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class Empresa
{
    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string? Direccion { get; set; }

    public string? Teléfono { get; set; }

    public virtual ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<LogError> LogErrors { get; set; } = new List<LogError>();

    public virtual ICollection<LogTransaccione> LogTransacciones { get; set; } = new List<LogTransaccione>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
