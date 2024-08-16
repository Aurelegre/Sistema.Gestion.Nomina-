﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<Ausencia> Ausencias { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Familia> Familias { get; set; }

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

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=AURELIUS\\SQLEXPRESS; Database=Sistema.Gestion.Nomina; Trusted_Connection=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ausencia>(entity =>
        {
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaSolicitud).HasColumnType("datetime");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Ausencia)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Ausencias_Empleados");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.IdEmpresa)
                .HasConstraintName("FK_Departamentos_Empresas");
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
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.Property(e => e.Nombre).IsUnicode(false);
            entity.Property(e => e.Teléfono)
                .HasMaxLength(50)
                .HasColumnName("teléfono");
        });

        modelBuilder.Entity<Familia>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Parentesco).HasMaxLength(50);

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Familia)
                .HasForeignKey(d => d.IdEmpleado)
                .HasConstraintName("FK_Familias_Empleados");
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
            entity.Property(e => e.SueldoExtra).HasColumnType("decimal(18, 2)");

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
            entity.HasNoKey();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

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
