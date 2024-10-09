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
                // Consulta el usuario y el empleado en una sola operación
                var empleado = await context.Empleados
                    .Include(u => u.IdUsuarioNavigation) // Si hay una relación entre Usuario y Empleado
                    .SingleOrDefaultAsync(u => u.IdUsuarioNavigation.Usuario1 == userName);

                // Verificar si el empleado existe
                if (empleado == null || empleado.IdUsuarioNavigation == null)
                {
                    return new LoginModel
                    {
                        Usuario = null,
                        isBloqued = false
                    };
                }

                // Asignar el usuario a la variable usuario
                var usuario = empleado.IdUsuarioNavigation;

                // Si la contraseña es incorrecta
                if (!hasher.VerifyPassword(password, usuario.Contraseña))
                {
                    // Incrementa los intentos
                    usuario.Attempts++;
                    bool isBlocked = await CheckAndHandleUserBlocking(usuario);

                    return new LoginModel
                    {
                        Usuario = null,
                        isBloqued = isBlocked
                    };
                }

                // Si la contraseña es correcta, restablece los intentos
                if (usuario.Attempts > 0)
                {
                    usuario.Attempts = 0;
                    context.Usuarios.Update(usuario);
                }

                await context.SaveChangesAsync(); // Guardar solo una vez

                // Obtener el id del empleado
                int idEmployee = empleado.Id;

                return new LoginModel
                {
                    Usuario = usuario,
                    isBloqued = false,
                    IdEmployee = idEmployee
                };
            }
            catch (Exception ex)
            {
                await logServices.LogError(1, 1, "LoginUser", $"Error al iniciar sesión del usuario {userName}", ex.Message, ex.StackTrace);
                return null;
            }
        }

        private async Task<bool> CheckAndHandleUserBlocking(Usuario usuario)
        {
            bool result = false;
            int maxAttempts = 3; // Hacer configurable
            if (usuario.Attempts >= maxAttempts && usuario.activo == 1)
            {
                // Bloquear al usuario si supera el máximo de intentos
                usuario.activo = 0;
                result = true;
            }
            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync(); // Es mejor hacer un guardado asíncrono
            return result;
        }


        public async Task<List<string>> GetUserPermission(int? idRol)
        {
            try
            {
                List<string> userPermissions = new List<string>();
                // Obtener los permisos del rol del usuario
                var permissions = await context.RolesPermisos.Include(p => p.IdPermisoNavigation)
                                          .Where(p => p.IdRol == idRol && p.IdPermisoNavigation.Padre != null)
                                          .Select(p => new { p.IdPermisoNavigation.Nombre, p.IdPermisoNavigation.Padre })
                                          .ToListAsync();
                foreach (var permission in permissions)
                {
                    var padre = await context.Permisos.SingleAsync(p => p.Id == permission.Padre);
                    userPermissions.Add($"{padre.Nombre}.{permission.Nombre}");
                }
                return userPermissions;
            }
            catch (Exception ex)
            {
                await logServices.LogError(0, 0, "GetUserPermission",  $"Error al consultar permisos del rol: {idRol}", ex.Message, ex.StackTrace);
                return null;
            }

        }
        
        public async Task<List<string>> GetallPermissions()
        {
            try
            {
                List<string> allPermissions = new List<string> ();
               var permissions = await context.Permisos
                                    .Where(p=> p.Padre !=null)
                                    .Select( p => new { p.Nombre, p.Padre })
                                    .ToListAsync();
                foreach (var permission in permissions)
                {
                    var padre = await context.Permisos.SingleAsync(p => p.Id == permission.Padre);
                    allPermissions.Add($"{padre.Nombre}.{permission.Nombre}");
                }
                return allPermissions;
            }
            catch (Exception ex) 
            {
                await logServices.LogError(1, 1, "GetallPermissions", "Error al consultar todos los permisos del sistema", ex.Message, ex.StackTrace);
                return new List<string> ();
            }
        }
        public async Task ConfigurePermissions()
        {
            try
            {
                List<string> permissions = await  GetallPermissions();
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
                await logServices.LogError(1, 1, "ConfigurePermissions", $"Error guardar permisos del usuario", ex.Message, ex.StackTrace);
            }
        }
    }
}
