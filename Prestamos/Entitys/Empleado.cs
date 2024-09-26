﻿using System;
using System.Collections.Generic;

namespace Prestamos.Entitys;

public partial class Empleado
{
    public int Id { get; set; }

    public string? Dpi { get; set; }

    public string? Nombre { get; set; }

    public DateTime? FechaContratado { get; set; }

    public int? IdUsuario { get; set; }

    public decimal? Sueldo { get; set; }

    public int? IdPuesto { get; set; }

    public int? IdEmpresa { get; set; }

    public int? Activo { get; set; }

    public virtual ICollection<Ausencia> Ausencia { get; set; } = new List<Ausencia>();

    public virtual ICollection<Familia> Familia { get; set; } = new List<Familia>();

    public virtual ICollection<HistorialPago> HistorialPagos { get; set; } = new List<HistorialPago>();

    public virtual ICollection<HistorialSueldo> HistorialSueldos { get; set; } = new List<HistorialSueldo>();

    public virtual Empresa? IdEmpresaNavigation { get; set; }

    public virtual Puesto? IdPuestoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<LogError> LogErrors { get; set; } = new List<LogError>();

    public virtual ICollection<LogTransaccione> LogTransacciones { get; set; } = new List<LogTransaccione>();

    public virtual ICollection<Nomina> Nominas { get; set; } = new List<Nomina>();

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}