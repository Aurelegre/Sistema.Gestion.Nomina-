using Microsoft.AspNetCore.Mvc;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Controller]
    public class LoginController : Controller
    {

        public LoginController()
        {
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
