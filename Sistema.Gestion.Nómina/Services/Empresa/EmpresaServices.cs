using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Services.Logs;

namespace Sistema.Gestion.Nómina.Services.Empresa
{
    public class EmpresaServices(SistemaGestionNominaContext context, Hasher hasher, ILogServices logger) : IEmpresaServices
    {
        public async Task<int> CreateAdmin(int idEmpresa, string nombre, string dpi, int idUsuario)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                
                try{
                    Empleado admin = new Empleado
                    {
                        Dpi = dpi,
                        Nombre = nombre,
                        FechaContratado = DateTime.Now,
                        IdUsuario = idUsuario,
                        Sueldo = 1,
                        IdPuesto = null,
                        IdDepartamento = null,
                        IdEmpresa = idEmpresa,
                        Activo = 1,
                        Apellidos = "Administrador"
                    };

                    context.Empleados.Add(admin);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return admin.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(1, 1, "CreateAdmin", $"Error al crear usuario admin de la Empresa: {idEmpresa}", ex.Message, ex.StackTrace);
                    return 0;
                }

            }
            
        }

        public async Task<int> CreateUser(int idEmpresa, string usuario, string contraseña)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                
                try{
                    Role roladmin = new Role
                    {
                        Descripcion = "Administrador",
                        IdEmpresa = idEmpresa,
                        activo = 1,
                    };
                    context.Roles.Add(roladmin);
                    await context.SaveChangesAsync();
                    //traer todos los roles
                    var permisos = await context.Permisos.AsNoTracking().ToListAsync();
                    foreach (var permiso in permisos)
                    {
                        RolesPermiso rolesPermiso = new RolesPermiso
                        {
                            IdPermiso = permiso.Id,
                            IdRol = roladmin.Id,
                        };
                        context.RolesPermisos.Add(rolesPermiso);
                    }
                    await context.SaveChangesAsync();
                    //crear usuario
                    Usuario usuarioAdmin = new Usuario
                    {
                        Usuario1 = usuario,
                        Contraseña = hasher.HashPassword(contraseña),
                        IdRol = roladmin.Id,
                        IdEmpresa = idEmpresa,
                        activo = 1,
                        Attempts = 0
                    };
                    context.Usuarios.Add(usuarioAdmin);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return usuarioAdmin.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await logger.LogError(1, 1, "CreateUser", $"Error al crear Rol y Usuario de la Empresa: {idEmpresa}", ex.Message, ex.StackTrace);
                    return 0;
                }
            }
            
        }
    }
}
