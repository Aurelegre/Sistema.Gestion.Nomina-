using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services;
using System.Security.Claims;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        private readonly SistemaGestionNominaContext context;

        public LoginController(LoginService loginService, SistemaGestionNominaContext context)
        {
            _loginService = loginService;
            this.context = context;
        }

        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login (LoginModel request)
        {
            var usuario = await _loginService.LoginUser(request.user,request.password);
            if(usuario != null)
            {
                var rol = await context.Roles.SingleAsync(r => r.Id == usuario.IdRol);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Usuario1),
                    new Claim(ClaimTypes.Role, rol.Descripcion)
                };

                var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

              return  RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("200", "Usuario o contraseña incorrectos.");
            return View();
        }

        public async Task<IActionResult> LogOut()
        {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Login");
        }
    }
}
