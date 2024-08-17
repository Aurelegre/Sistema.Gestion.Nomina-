using System;
using System.Collections.Generic;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class Departamento
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }

    public int? IdJefe { get; set; }

    public int? IdEmpresa { get; set; }

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual ICollection<Puesto> Puestos { get; set; } = new List<Puesto>();
}
