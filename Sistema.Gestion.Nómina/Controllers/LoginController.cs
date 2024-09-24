using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services;
using Sistema.Gestion.Nómina.Services.Logs;
using System.Security.Claims;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        private readonly SistemaGestionNominaContext context;
        private readonly IServiceCollection _services;
        public ILogServices LogServices { get; }

        public LoginController(LoginService loginService, SistemaGestionNominaContext context, IServiceCollection services, ILogServices logServices)
        {
            _loginService = loginService;
            this.context = context;
            _services = services;
            LogServices = logServices;
        }

        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login (LoginModel request)
        {
            try
            {
                //estandarización
                request.user = request.user.ToUpper();
                var usuario = await _loginService.LoginUser(request.user, request.password);
                if (usuario == null)
                {
                    ModelState.AddModelError("400", "Usuario o contraseña incorrectos.");
                    return View();
                }

                var rol = await context.Roles.SingleAsync(r => r.Id == usuario.IdRol);
                var permissions = _loginService.GetsessionPermission(rol.Id);
                _loginService.ConfigureServices(_services);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Usuario1),
                    new Claim(ClaimTypes.Role, rol.Descripcion),
                    new Claim("Company", usuario.IdEmpresa.ToString() ),
                    new Claim("IdEmployed", usuario.Id.ToString())
                };

                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                //registro de bítacora
                var empleado = await context.Empleados.Where(e=> e.IdUsuario == usuario.Id).FirstOrDefaultAsync();
                await LogServices.LogTransaction(empleado.Id, usuario.IdEmpresa, "Login", "Inicio de sesión", usuario.Usuario1);

                return RedirectToAction("Index", "Home");
            }catch (Exception ex)
            {
                await LogServices.LogError(1, 1, "Login", "Error al inciar sessión", ex.Message, ex.StackTrace);
                ModelState.AddModelError("400", "ERROR al iniciar sesión.");
                return View();
            }
        }

        public async Task<IActionResult> LogOut()
        {
            try 
            { 
                

                //registro de bítacora
                var session = LogServices.GetSessionData();
                await LogServices.LogTransaction(session.idEmpleado, session.company, "LogOut", "Se cerró de sesión", session.nombre);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Login");
            }catch  (Exception ex) 
            { 
                //registro de bítacora
                var session = LogServices.GetSessionData();
                await LogServices.LogError(session.idEmpleado, session.company, "LogOut", $"Error al cerrar sessión del usuario: {session.nombre}", ex.Message, ex.StackTrace);
                ModelState.AddModelError("400", "ERROR al iniciar sesión.");
                return RedirectToAction("Index", "Home");
            }
            
        }

        public async Task<IActionResult> AccessDenied ()
        {

            return View();
        }
    }
}
