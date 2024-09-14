using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Services.Logs;
using System.Security.Policy;

namespace Sistema.Gestion.Nómina.Services
{
    public class LoginService
    {
        private readonly SistemaGestionNominaContext _context;
        private readonly Hasher hasher;
        private readonly IServiceProvider _services;

        public ILogServices logServices { get; }

        public LoginService(SistemaGestionNominaContext dbcontext, Hasher hasher, IServiceProvider services, ILogServices logServices)
        {
            _context = dbcontext;
            this.hasher = hasher;
            _services = services;
            this.logServices = logServices;
        }

        public async Task<Usuario> LoginUser(string? userName, string? password)
        {
            try
            {
                var usuario = await _context.Usuarios.SingleOrDefaultAsync(u => u.Usuario1 == userName);

                if (usuario == null || !hasher.VerifyPassword(password, usuario.Contraseña))
                {
                    return null;
                }

                return usuario;
            }catch (Exception ex)
            {
                logServices.LogError(0, 0, "LoginUser", $"Error en el servicio inicial para inciar sessión del usuario {userName} ", ex.Message, ex.StackTrace);
                return null;
            }
           
        }

        public List<string> GetsessionPermission(int idRol)
        {
            try
            {
                List<string> userPermissions = new List<string>();
                // Obtener los permisos del rol del usuario
                var permissions = _context.RolesPermisos.Include(p => p.IdPermisoNavigation)
                                          .Where(p => p.IdRol == idRol && p.IdPermisoNavigation.Padre != null)
                                          .Select(p => new { p.IdPermisoNavigation.Nombre, p.IdPermisoNavigation.Padre })
                                          .ToList();
                foreach (var permission in permissions)
                {
                    var padre = _context.Permisos.Single(p => p.Id == permission.Padre);
                    userPermissions.Add($"{padre.Nombre}.{permission.Nombre}");
                }
                return userPermissions;
            }
            catch (Exception ex)
            {
                logServices.LogError(0, 0, "GetsessionPermission",  $"Error al consultar permisos del rol: {idRol}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        
        public List<string> GetallPermissions()
        {
            try
            {
                List<string> allPermissions = new List<string> ();
               var permissions = _context.Permisos
                                    .Where(p=> p.Padre !=null)
                                    .Select( p => new { p.Nombre, p.Padre }).ToList();
                foreach (var permission in permissions)
                {
                    var padre = _context.Permisos.Single(p => p.Id == permission.Padre);
                    allPermissions.Add($"{padre.Nombre}.{permission.Nombre}");
                }
                return allPermissions;
            }
            catch (Exception ex) 
            {
                logServices.LogError(0, 0, "GetallPermissions", "Error al consultar todos los permisos del sistema", ex.Message, ex.StackTrace);
                return new List<string> ();
            }
        }
        public void ConfigureServices( IServiceCollection services)
        {
            try
            {
                List<string> permissions = GetallPermissions();
                using var scope = _services.CreateScope();
                var authorizationOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

                // Registrar políticas dinámicamente
                foreach (var permission in permissions)
                {
                        authorizationOptions.AddPolicy(permission, policy =>
                            policy.RequireClaim("Permission", permission));
                }
            }
            catch (Exception ex)
            {
                logServices.LogError(0, 0, "ConfigureServices", $"Error guardar permisos del usuario", ex.Message, ex.StackTrace);
            }
        }
    }
}
