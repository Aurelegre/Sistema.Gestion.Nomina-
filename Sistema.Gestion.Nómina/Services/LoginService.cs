using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Sistema.Gestion.Nómina.DTOs.Login;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Services.Logs;
using System.Security.Policy;

namespace Sistema.Gestion.Nómina.Services
{
    public class LoginService
    {
        private readonly SistemaGestionNominaContext context;
        private readonly Hasher hasher;
        private readonly IServiceProvider _services;

        public ILogServices logServices { get; }

        public LoginService(SistemaGestionNominaContext dbcontext, Hasher hasher, IServiceProvider services, ILogServices logServices)
        {
            context = dbcontext;
            this.hasher = hasher;
            _services = services;
            this.logServices = logServices;
        }

        public async Task<LoginModel> LoginUser(string? userName, string? password)
        {
            try
            {
                var usuario = await context.Usuarios.SingleOrDefaultAsync(u => u.Usuario1 == userName);

                // el usuario no existe
                if (usuario == null)
                {
                    return new LoginModel
                    {
                        Usuario = null,
                        isBloqued = false
                    };
                }
                if (!hasher.VerifyPassword(password, usuario.Contraseña))
                {
                    //incrementar los intentos
                    usuario.Attempts++;
                    context.Usuarios.Update(usuario);
                     await context.SaveChangesAsync();
                    //contraseña incorrecta, conteo de intentos
                    bool isBloqued = CountAttempts(usuario);//devuelve true si se alcazó el maximo y bloque de usuario

                    return new LoginModel {
                        Usuario = null,
                        isBloqued =isBloqued
                    }; 
                }
                //verificar el número de intentos
                if (!CountAttempts(usuario))
                {
                    //contraseña correcta, resetear intentos
                    usuario.Attempts =  0;
                    context.Usuarios.Update(usuario);
                    await context.SaveChangesAsync();
                }
                return new LoginModel{
                    Usuario = usuario,
                    isBloqued = false
                };
            }catch (Exception ex)
            {
                 await logServices.LogError(1, 1, "LoginUser", $"Error en el servicio inicial para inciar sessión del usuario {userName} ", ex.Message, ex.StackTrace);
                return null;
            }
        }

        private bool CountAttempts(Usuario usuario)
        {
            
                int maxAttempts = 3; //hacer configurable
                if (usuario.Attempts > maxAttempts && usuario.activo == 1)
                {
                    // si es mayor bloquear usuario
                    usuario.activo = 0;
                    context.Usuarios.Update(usuario);
                    context.SaveChanges();
                    return true;
                }
                return false;
        }

        public List<string> GetsessionPermission(int idRol)
        {
            try
            {
                List<string> userPermissions = new List<string>();
                // Obtener los permisos del rol del usuario
                var permissions = context.RolesPermisos.Include(p => p.IdPermisoNavigation)
                                          .Where(p => p.IdRol == idRol && p.IdPermisoNavigation.Padre != null)
                                          .Select(p => new { p.IdPermisoNavigation.Nombre, p.IdPermisoNavigation.Padre })
                                          .ToList();
                foreach (var permission in permissions)
                {
                    var padre = context.Permisos.Single(p => p.Id == permission.Padre);
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
               var permissions = context.Permisos
                                    .Where(p=> p.Padre !=null)
                                    .Select( p => new { p.Nombre, p.Padre }).ToList();
                foreach (var permission in permissions)
                {
                    var padre = context.Permisos.Single(p => p.Id == permission.Padre);
                    allPermissions.Add($"{padre.Nombre}.{permission.Nombre}");
                }
                return allPermissions;
            }
            catch (Exception ex) 
            {
                logServices.LogError(1, 1, "GetallPermissions", "Error al consultar todos los permisos del sistema", ex.Message, ex.StackTrace);
                return new List<string> ();
            }
        }
        public void ConfigurePermissions( IServiceCollection services)
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
                logServices.LogError(1, 1, "ConfigurePermissions", $"Error guardar permisos del usuario", ex.Message, ex.StackTrace);
            }
        }
    }
}
