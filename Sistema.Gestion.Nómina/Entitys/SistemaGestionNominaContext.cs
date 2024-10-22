using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;

namespace Sistema.Gestion.Nómina.Entitys;

public partial class SistemaGestionNominaContext : DbContext
{
    public SistemaGestionNominaContext()
    {
    }

    public SistemaGestionNominaContext(DbContextOptions<SistemaGestionNominaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aumento> Aumento { get; set; }
    public virtual DbSet<Ausencia> Ausencias { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }
    public virtual DbSet<Descuento> Descuento { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Familia> Familias { get; set; }

    public virtual DbSet<HistorialPago> HistorialPagos { get; set; }

    public virtual DbSet<HistorialSueldo> HistorialSueldos { get; set; }

    public virtual DbSet<LogError> LogErrors { get; set; }

    public virtual DbSet<LogTransaccione> LogTransacciones { get; set; }

    public virtual DbSet<Nomina> Nominas { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Prestamo> Prestamos { get; set; }

    public virtual DbSet<Puesto> Puestos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolesPermiso> RolesPermisos { get; set; }

    public virtual DbSet<TiposPrestamo> TiposPrestamos { get; set; }
    public virtual DbSet<TipoDescuento> TipoDescuento { get; set; }
    public virtual DbSet<TipoAumento> TipoAumento { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HistorialSolicitudesModel>().HasNoKey();
        modelBuilder.Entity<Aumento>(entity =>
        {
            // Configuración de las columnas
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            // Relación con la tabla Empleados
            entity.HasOne(d => d.IdEmpleadoNavigation)
                .WithMany(p => p.Aumento)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Aumentos_Empleados");

            // Relación con la tabla TipoAumento
            entity.HasOne(d => d.IdTipoNavigation)
                .WithMany(p => p.Aumento)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Aumentos_TipoAumento");
        });
        
        modelBuilder.Entity<Ausencia>(entity =>
        {
            entity.Property(e => e.Detalle).IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaSolicitud).HasColumnType("datetime");
            entity.Property(e => e.FechaAutorizado).HasColumnType("datetime");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Ausencia)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Ausencias_Empleados");
            entity.HasOne(d => d.idJefeNavigation).WithMany(p => p.AusenciaJefe)
                .HasForeignKey(d => d.idJefe)
                .HasConstraintName("FK_Ausencias_Jefe");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Departamentos_Empresas");
            entity.HasOne(d => d.IdJefeNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdJefe)
                .HasConstraintName("FK_Departamentos_Empleados");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.Property(e => e.Dpi)
                .HasMaxLength(50)
                .HasColumnName("DPI");
            entity.Property(e => e.FechaContratado).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Sueldo).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Empleados_Empresas");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdPuesto)
                .HasConstraintName("FK_Empleados_Puesto");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Empleados_Usuarios");
            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdDepartamento)
                .HasConstraintName("FK_Empleados_Departamentos");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.Property(e => e.Nombre).IsUnicode(false);
            entity.Property(e => e.Teléfono)
                .HasMaxLength(50)
                .HasColumnName("teléfono");
        });
        modelBuilder.Entity<Descuento>(entity =>
        {
            // Configuración de las columnas
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

            // Relación con la tabla Empleados
            entity.HasOne(d => d.IdEmpleadoNavigation)
                .WithMany(p => p.Descuento)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Descuento_Empleados");

            // Relación con la tabla TipoAumento
            entity.HasOne(d => d.IdTipoNavigation)
                .WithMany(p => p.Descuento)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Descuento_TipoDescuento");
        });

        modelBuilder.Entity<Familia>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Parentesco).HasMaxLength(50);

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Familia)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Familias_Empleados");
        });

        modelBuilder.Entity<HistorialPago>(entity =>
        {
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.TotalPagado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPendiente).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_HistorialPagos_Empleados");

            entity.HasOne(d => d.IdPrestamoNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdPrestamo)
                .HasConstraintName("FK_HistorialPagos_Prestamos");
        });

        modelBuilder.Entity<HistorialSueldo>(entity =>
        {
            entity.Property(e => e.AnteriorSalario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.NuevoSalario).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.HistorialSueldos)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_HistorialSueldos_Empleados");
        });

        modelBuilder.Entity<LogError>(entity =>
        {
            entity.ToTable("LogError");

            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.LogErrors)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_LogError_Empleados");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.LogErrors)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_LogError_Empresas");
        });

        modelBuilder.Entity<LogTransaccione>(entity =>
        {
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Metodo).HasMaxLength(50);
            entity.Property(e => e.Usuario).HasMaxLength(80);

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.LogTransacciones)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_LogTransacciones_Empleados");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.LogTransacciones)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_LogTransacciones_Empresas");
        });

        modelBuilder.Entity<Nomina>(entity =>
        {
            entity.Property(e => e.Anticipos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Bonificaciones).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Comisiones).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Igss)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IGSS");
            entity.Property(e => e.Isr)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ISR");
            entity.Property(e => e.OtrosDesc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OtrosIngresos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Prestamos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Creditos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SueldoExtra).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sueldo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDevengado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDescuentos).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalLiquido).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AguinaldoBono).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Nominas_Empleados");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(50);

            entity.HasOne(d => d.PadreNavigation).WithMany(p => p.InversePadreNavigation)
                .HasForeignKey(d => d.Padre)
                .HasConstraintName("FK_Permisos_Permisos");
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.Property(e => e.FechaPrestamo).HasColumnType("datetime");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPendiente).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Prestamos_Empleados");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Prestamos)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Prestamos_TiposPrestamo");
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.ToTable("Puesto");

            entity.Property(e => e.Descripcion).HasMaxLength(50);

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Puestos)
                .HasForeignKey(d => d.IdDepartamento)
                .HasConstraintName("FK_Puesto_Departamentos");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(50);

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Roles)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Roles_Empresas");
        });

        modelBuilder.Entity<RolesPermiso>(entity =>
        {
            

            entity.HasOne(d => d.IdPermisoNavigation).WithMany()
                .HasForeignKey(d => d.IdPermiso)
                .HasConstraintName("FK_RolesPermisos_Permisos");

            entity.HasOne(d => d.IdRolNavigation).WithMany()
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_RolesPermisos_Roles");
        });

        modelBuilder.Entity<TiposPrestamo>(entity =>
        {
            entity.ToTable("TiposPrestamo");

            entity.Property(e => e.Descripcion).HasMaxLength(50);
        });
        modelBuilder.Entity<TipoAumento>(entity =>
        {
            entity.ToTable("TipoAumento");

            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });
        modelBuilder.Entity<TipoDescuento>(entity =>
        {
            entity.ToTable("TipoDescuento");

            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Usuario1).HasColumnName("Usuario");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Usuarios_Empresas");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
