using Microsoft.AspNetCore.Mvc;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
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
              return  RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("200", "Usuario o contraseña incorrectos.");
            return View();
        }
    }
}
