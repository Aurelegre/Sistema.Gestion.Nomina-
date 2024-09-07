using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Role
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }

    public int? IdEmpresa { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
